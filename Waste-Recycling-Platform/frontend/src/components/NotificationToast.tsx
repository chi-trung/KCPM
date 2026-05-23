'use client';

import { useEffect, useState } from 'react';
import { useSignalR, NotificationPayload } from '@/hooks/useSignalR';
import { useAuth } from '@/contexts/AuthContext';
import { Bell, X, CheckCircle, Info, AlertCircle } from 'lucide-react';
import Link from 'next/link';

interface Toast {
  id: string;
  notification: NotificationPayload;
}

export function NotificationToast() {
  const { user } = useAuth();
  const [toasts, setToasts] = useState<Toast[]>([]);
  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

  const { isConnected } = useSignalR({
    enabled: !!user && !!token,
    token,
    onNewNotification: (notification) => {
      // Add toast
      setToasts(prev => [...prev, { id: notification.id, notification }]);
      
      // Auto remove after 5 seconds
      setTimeout(() => {
        setToasts(prev => prev.filter(t => t.id !== notification.id));
      }, 5000);
    }
  });

  const removeToast = (id: string) => {
    setToasts(prev => prev.filter(t => t.id !== id));
  };

  const getIcon = (type: string) => {
    switch (type) {
      case 'ReportCollected':
        return <CheckCircle className="w-5 h-5 text-green-500" />;
      case 'ReportRejected':
        return <AlertCircle className="w-5 h-5 text-red-500" />;
      default:
        return <Info className="w-5 h-5 text-blue-500" />;
    }
  };

  if (toasts.length === 0) return null;

  return (
    <div className="fixed top-4 right-4 z-50 space-y-2">
      {toasts.map(({ id, notification }) => (
        <div
          key={id}
          className="bg-white rounded-lg shadow-lg border border-gray-200 p-4 min-w-[300px] max-w-[400px] animate-in slide-in-from-right"
        >
          <div className="flex items-start gap-3">
            {getIcon(notification.type)}
            <div className="flex-1">
              <h4 className="font-semibold text-gray-900">{notification.title}</h4>
              <p className="text-sm text-gray-600 mt-1">{notification.message}</p>
            </div>
            <button
              onClick={() => removeToast(id)}
              className="text-gray-400 hover:text-gray-600"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
