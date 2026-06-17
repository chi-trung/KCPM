// frontend/src/components/shared/NotificationCenter.tsx

'use client';

import React, { useState, useEffect, useCallback, useRef } from 'react';
import { Bell, X } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { useSignalR, NotificationPayload } from '@/hooks/useSignalR';
import { API_CONFIG } from '@/lib/api/config';

interface Notification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: Date;
  isRead: boolean;
  relatedEntityType?: string;
  relatedEntityId?: string;
}

export const NotificationCenter: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const { user, token } = useAuth();
  const router = useRouter();
  const dropdownRef = useRef<HTMLDivElement>(null);
  const unreadCount = notifications.filter(n => !n.isRead).length;

  // Fetch notifications from API
  const fetchNotifications = useCallback(async () => {
    if (!token) return;
    try {
      const res = await fetch(`${API_CONFIG.SERVER_URL}/api/notifications`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (res.ok) {
        const data = await res.json();
        const apiNotifications = data.data?.map((n: any) => ({
          id: n.id,
          title: n.title,
          message: n.message,
          type: mapType(n.type),
          timestamp: new Date(n.createdAt),
          isRead: n.status === 'Read',
          relatedEntityType: n.relatedEntityType,
          relatedEntityId: n.relatedEntityId
        })) || [];
        setNotifications(apiNotifications);
      }
    } catch (err) {
      console.error('Failed to fetch notifications', err);
    }
  }, [token]);

  // Map backend type to frontend type
  const mapType = (type: string | number | null | undefined): 'info' | 'success' | 'warning' | 'error' => {
    const typeStr = typeof type === 'string' ? type.toLowerCase() : String(type).toLowerCase();
    switch (typeStr) {
      case 'success': return 'success';
      case 'warning': return 'warning';
      case 'error': return 'error';
      default: return 'info';
    }
  };

  // Initial fetch
  useEffect(() => {
    fetchNotifications();
  }, [fetchNotifications]);

  // Real-time notifications via SignalR
  useSignalR({
    enabled: !!token,
    token,
    onNewNotification: (payload: NotificationPayload) => {
      const newNotification: Notification = {
        id: payload.id,
        title: payload.title,
        message: payload.message,
        type: mapType(payload.type),
        timestamp: new Date(payload.createdAt),
        isRead: false
      };
      setNotifications(prev => [newNotification, ...prev]);
    }
  });

  const handleNotificationClick = async (notif: Notification) => {
    if (!token) return;
    
    // Mark as read
    try {
      await fetch(`${API_CONFIG.SERVER_URL}/api/notifications/${notif.id}/read`, {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      await fetchNotifications();
    } catch (err) {
      console.error('Failed to mark as read', err);
    }
    
    // Navigate based on notification type
    if (notif.relatedEntityType?.toLowerCase() === 'complaint') {
      router.push('/citizen/complaints');
    }
    
    setIsOpen(false);
  };

  const markAllAsRead = async () => {
    if (!token) return;
    try {
      await fetch(`${API_CONFIG.SERVER_URL}/api/notifications/mark-all-read`, {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      await fetchNotifications();
    } catch (err) {
      console.error('Failed to mark all as read', err);
    }
  };

  // Refresh when dropdown opens
  useEffect(() => {
    if (isOpen) {
      fetchNotifications();
    }
  }, [isOpen, fetchNotifications]);

  const removeNotification = (id: string) => {
    setNotifications(prev => prev.filter((n: Notification) => n.id !== id));
  };

  // Click outside to close dropdown
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  return (
    <div ref={dropdownRef} className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-gray-400 hover:text-gray-600"
      >
        <Bell size={20} />
        {unreadCount > 0 && (
          <span className="absolute top-1 right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
            {unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-xl z-50 max-h-96 overflow-y-auto">
          {/* Header with mark all as read */}
          <div className="flex justify-between items-center p-3 border-b bg-gray-50 rounded-t-lg">
            <span className="font-semibold text-gray-700">Thông báo</span>
            {unreadCount > 0 && (
              <button
                onClick={markAllAsRead}
                className="text-xs text-blue-600 hover:text-blue-800 font-medium"
              >
                Đánh dấu tất cả đã đọc
              </button>
            )}
          </div>

          {notifications.length === 0 ? (
            <div className="p-4 text-center text-gray-500">
              Không có thông báo nào
            </div>
          ) : (
            notifications.map((notif: Notification) => (
              <button
                key={notif.id}
                type="button"
                onClick={() => handleNotificationClick(notif)}
                className={`w-full text-left p-4 border-b hover:bg-gray-100 cursor-pointer transition-colors ${
                  !notif.isRead ? 'bg-blue-50' : 'bg-white'
                }`}
              >
                <div className="flex items-start gap-3">
                  {/* Unread indicator dot */}
                  {!notif.isRead && (
                    <span className="mt-1.5 w-2 h-2 bg-blue-500 rounded-full flex-shrink-0" />
                  )}
                  <div className="flex-1">
                    <h4 className={`font-semibold ${!notif.isRead ? 'text-gray-900' : 'text-gray-600'}`}>
                      {notif.title}
                    </h4>
                    <p className={`text-sm mt-1 ${!notif.isRead ? 'text-gray-700' : 'text-gray-500'}`}>
                      {notif.message}
                    </p>
                    <time className="text-xs text-gray-400 mt-2 block">
                      {notif.timestamp.toLocaleString('vi-VN')}
                    </time>
                  </div>
                </div>
              </button>
            ))
          )}
        </div>
      )}
    </div>
  );
};