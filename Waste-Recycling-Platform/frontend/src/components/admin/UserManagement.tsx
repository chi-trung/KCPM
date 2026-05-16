"use client";
import React, { useState, useEffect, useCallback } from 'react';
import { API_CONFIG } from '@/lib/api/config';
import { Table } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { Edit3, ShieldAlert, UserCheck, Search, X, AlertCircle, CheckCircle2, Filter, MoreHorizontal, Mail, Shield } from 'lucide-react';
import { Portal } from '../shared/Portal';

interface User {
  id: string;
  fullName: string;
  email: string;
  role: 'admin' | 'citizen' | 'collector' | 'enterprise';
  isActive: boolean;
  lastActiveDate: string;
}

export const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState('all');

  const [confirmModal, setConfirmModal] = useState({ isOpen: false, userId: '', isActive: false });
  const [isToggling, setIsToggling] = useState(false);
  const [editRoleModal, setEditRoleModal] = useState({ isOpen: false, userId: '', role: '' });
  const [isUpdatingRole, setIsUpdatingRole] = useState(false);

  const fetchUsers = useCallback(async (silentLoad = false) => {
    if (!silentLoad) setIsLoading(true);
    try {
      const queryParams = new URLSearchParams();
      if (searchTerm) queryParams.append('search', searchTerm);
      if (roleFilter !== 'all') queryParams.append('role', roleFilter);

      const response = await fetch(`${API_CONFIG.BASE_URL}/admin/users?${queryParams.toString()}`);
      if (response.ok) {
        const result = await response.json();
        setUsers(result.data || []);
      } else {
        setUsers([]);
      }
    } catch (error) {
      console.error('Lỗi:', error);
    } finally {
      if (!silentLoad) setIsLoading(false);
    }
  }, [searchTerm, roleFilter]);

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      fetchUsers(false);
    }, 500);
    return () => clearTimeout(timeoutId);
  }, [fetchUsers]);

  const executeToggleStatus = async () => {
    setIsToggling(true);
    try {
      const response = await fetch(`${API_CONFIG.BASE_URL}/admin/users/${confirmModal.userId}/toggle-status`, {
        method: 'PATCH',
      });
      if (response.ok) {
        setConfirmModal({ isOpen: false, userId: '', isActive: false });
        fetchUsers(true);
      }
    } finally {
      setIsToggling(false);
    }
  };

  const executeUpdateRole = async () => {
    setIsUpdatingRole(true);
    try {
      const response = await fetch(`${API_CONFIG.BASE_URL}/admin/users/${editRoleModal.userId}/role`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ newRole: editRoleModal.role })
      });
      if (response.ok) {
        setEditRoleModal({ isOpen: false, userId: '', role: '' });
        fetchUsers(true);
      }
    } finally {
      setIsUpdatingRole(false);
    }
  };

  const getRoleStyle = (role: string) => {
    switch (role?.toLowerCase()) {
      case 'admin': return "bg-purple-50 text-purple-700 border-purple-100";
      case 'enterprise': return "bg-blue-50 text-blue-700 border-blue-100";
      case 'collector': return "bg-amber-50 text-amber-700 border-amber-100";
      default: return "bg-slate-50 text-slate-700 border-slate-100";
    }
  };

  const columns = [
    { 
      key: 'fullName', 
      label: 'Thành viên',
      render: (name: string, user: User) => (
        <div className="flex items-center gap-3 py-1">
          <div className="w-10 h-10 rounded-full bg-gradient-to-br from-emerald-400 to-teal-600 flex items-center justify-center text-white font-bold shadow-sm">
            {name.charAt(0).toUpperCase()}
          </div>
          <div>
            <div className="font-bold text-gray-900 text-sm">{name}</div>
            <div className="text-[11px] text-gray-400 flex items-center gap-1 font-medium">
              <Mail size={12} /> {user.email}
            </div>
          </div>
        </div>
      )
    },
    { 
      key: 'role', 
      label: 'Vai trò',
      render: (role: string) => (
        <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-[11px] font-extrabold border uppercase tracking-wider ${getRoleStyle(role)}`}>
          <Shield size={12} /> {role}
        </span>
      )
    },
    { 
      key: 'isActive', 
      label: 'Trạng thái',
      render: (isActive: boolean) => (
        <div className="flex items-center gap-2">
          <div className={`w-2 h-2 rounded-full animate-pulse ${isActive ? 'bg-emerald-500' : 'bg-red-400'}`} />
          <span className={`text-xs font-bold ${isActive ? 'text-emerald-600' : 'text-red-400'}`}>
            {isActive ? 'HOẠT ĐỘNG' : 'ĐÃ KHÓA'}
          </span>
        </div>
      )
    },
    { 
      key: 'lastActiveDate', 
      label: 'Truy cập cuối',
      render: (date: string) => (
        <span className="text-gray-400 text-xs font-medium italic">{date || 'Chưa có dữ liệu'}</span>
      )
    },
    {
      key: 'actions',
      label: '',
      render: (_: unknown, user: User) => (
        // Đã gỡ bỏ opacity-0 để nút luôn hiển thị rõ ràng
        <div className="flex gap-2 justify-end">
          <button 
            onClick={() => setEditRoleModal({ isOpen: true, userId: user.id, role: user.role })}
            className="p-2 hover:bg-gray-100 rounded-full text-gray-400 hover:text-blue-600 transition-colors"
            title="Sửa quyền"
          >
            <Edit3 size={16} />
          </button>
          <button 
            onClick={() => setConfirmModal({ isOpen: true, userId: user.id, isActive: user.isActive })}
            className={`p-2 rounded-full transition-colors ${
              user.isActive ? "text-gray-400 hover:text-red-600 hover:bg-red-50" : "text-gray-400 hover:text-emerald-600 hover:bg-emerald-50"
            }`}
          >
            {user.isActive ? <ShieldAlert size={16} /> : <UserCheck size={16} />}
          </button>
        </div>
      )
    }
  ];

  return (
    <div className="space-y-6 animate-in fade-in duration-700 pt-2">
      
      {/* Search & Filter Bar - Hiện đại, Tinh gọn */}
      <div className="bg-white rounded-2xl shadow-[0_8px_30px_rgb(0,0,0,0.04)] border border-gray-100 p-5">
        <div className="flex flex-col md:flex-row gap-4">
            <div className="relative flex-1 group">
                <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-300 group-focus-within:text-emerald-500 transition-colors" size={18} />
                <input 
                  type="text" 
                  placeholder="Tìm kiếm thành viên theo tên hoặc email..." 
                  value={searchTerm} 
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-12 pr-4 py-3 bg-gray-50 border-none rounded-2xl focus:ring-2 focus:ring-emerald-500/20 focus:bg-white transition-all text-sm font-medium placeholder:text-gray-300" 
                />
            </div>
            <div className="flex gap-3">
              <div className="relative min-w-[180px]">
                  <select 
                    value={roleFilter} 
                    onChange={(e) => setRoleFilter(e.target.value)}
                    className="w-full pl-4 pr-10 py-3 bg-gray-50 border-none rounded-2xl focus:ring-2 focus:ring-emerald-500/20 appearance-none cursor-pointer text-sm font-bold text-gray-600" 
                  >
                      <option value="all">Tất cả vai trò</option>
                      <option value="citizen">Người dân</option>
                      <option value="collector">Người thu gom</option>
                      <option value="enterprise">Doanh nghiệp</option>
                      {/* Vẫn giữ Admin ở Filter để tìm kiếm */}
                      <option value="admin">Quản trị viên</option>
                  </select>
                  <Filter className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-300 pointer-events-none" size={16} />
              </div>
            </div>
        </div>
      </div>

      {/* Table Section - Thoáng đãng */}
      <div className="bg-white rounded-2xl shadow-[0_8px_30px_rgb(0,0,0,0.04)] border border-gray-100 overflow-hidden">
        {isLoading ? (
            <div className="py-24 flex flex-col items-center justify-center">
                <div className="w-12 h-12 border-4 border-emerald-100 border-t-emerald-500 rounded-full animate-spin mb-4" />
                <p className="text-gray-400 text-sm font-medium animate-pulse">Đang tải dữ liệu người dùng...</p>
            </div>
        ) : users.length > 0 ? (
            <Table columns={columns} data={users} className="hover-rows" />
        ) : (
            <div className="p-20 text-center">
                <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center mx-auto mb-4 border border-dashed border-gray-200">
                  <X className="text-gray-300" size={32} />
                </div>
                <p className="text-gray-900 font-bold text-lg">Không tìm thấy ai</p>
                <p className="text-gray-400 text-sm mt-1">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm của bạn.</p>
            </div>
        )}
      </div>

      {/* MODAL CẢNH BÁO KHÓA - Design cực xịn */}
      {confirmModal.isOpen && (
        <Portal>
          <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-md flex items-center justify-center z-[100] animate-in fade-in duration-300">
            <div className="bg-white rounded-[32px] p-10 max-w-sm w-full mx-4 shadow-2xl animate-in zoom-in-95 duration-300 text-center">
              <div className={`mx-auto w-24 h-24 rounded-full flex items-center justify-center mb-6 shadow-xl ${confirmModal.isActive ? 'bg-red-50 text-red-500 shadow-red-100' : 'bg-emerald-50 text-emerald-500 shadow-emerald-100'}`}>
                {confirmModal.isActive ? <AlertCircle size={48} /> : <CheckCircle2 size={48} />}
              </div>
              <h3 className="text-2xl font-black text-gray-900 mb-3 tracking-tight">
                {confirmModal.isActive ? 'Khóa quyền?' : 'Mở quyền?'}
              </h3>
              <p className="text-gray-500 text-sm leading-relaxed mb-8">
                {confirmModal.isActive 
                  ? 'Tài khoản này sẽ bị đình chỉ và không thể truy cập hệ thống cho đến khi được mở lại.' 
                  : 'Xác nhận khôi phục quyền truy cập đầy đủ cho tài khoản này.'}
              </p>
              <div className="flex flex-col gap-3">
                <button 
                  onClick={executeToggleStatus} 
                  disabled={isToggling}
                  className={`w-full py-4 rounded-2xl font-black text-sm tracking-widest shadow-lg transition-all active:scale-95 ${confirmModal.isActive ? 'bg-red-500 hover:bg-red-600 text-white shadow-red-200' : 'bg-emerald-500 hover:bg-emerald-600 text-white shadow-emerald-200'}`}
                >
                  {isToggling ? 'ĐANG XỬ LÝ...' : 'XÁC NHẬN NGAY'}
                </button>
                <button 
                  onClick={() => setConfirmModal({ isOpen: false, userId: '', isActive: false })} 
                  className="w-full py-4 bg-gray-50 hover:bg-gray-100 text-gray-500 rounded-2xl font-bold text-sm transition-all"
                >
                  QUAY LẠI
                </button>
              </div>
            </div>
          </div>
        </Portal>
      )}

      {/* MODAL CẬP NHẬT QUYỀN - Grid Layout */}
      {editRoleModal.isOpen && (
        <Portal>
          <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-md flex items-center justify-center z-[100] animate-in fade-in duration-300">
            <div className="bg-white rounded-[32px] p-8 max-w-md w-full mx-4 shadow-2xl animate-in zoom-in-95 duration-300">
              <div className="flex items-center justify-between mb-8">
                <div className="flex items-center gap-4">
                  <div className="bg-blue-50 p-3 rounded-2xl text-blue-600">
                    <Shield size={24} />
                  </div>
                  <h3 className="text-xl font-black text-gray-900 tracking-tight uppercase">Phân quyền</h3>
                </div>
                <button onClick={() => setEditRoleModal({ isOpen: false, userId: '', role: '' })} className="p-2 hover:bg-gray-100 rounded-full text-gray-400 transition-colors"><X size={24} /></button>
              </div>
              
              <div className="space-y-4 mb-8">
                <p className="text-[10px] font-black text-gray-400 uppercase tracking-[2px] ml-1">Chọn cấp độ truy cập mới</p>
                <div className="grid grid-cols-1 gap-3">
                  {/* Đã gỡ 'admin' ra khỏi tùy chọn cấp quyền */}
                  {['enterprise', 'collector', 'citizen'].map((r) => (
                    <label key={r} className={`flex items-center justify-between p-4 rounded-2xl border-2 cursor-pointer transition-all ${editRoleModal.role === r ? 'border-blue-500 bg-blue-50/50 shadow-sm' : 'border-gray-100 bg-white hover:border-gray-200'}`}>
                      <span className="flex items-center gap-3">
                        <div className={`w-4 h-4 rounded-full border-2 flex items-center justify-center ${editRoleModal.role === r ? 'border-blue-500' : 'border-gray-300'}`}>
                          {editRoleModal.role === r && <div className="w-2 h-2 bg-blue-500 rounded-full" />}
                        </div>
                        {/* Translate sang tiếng Việt cho thân thiện */}
                        <span className={`text-sm font-bold capitalize ${editRoleModal.role === r ? 'text-blue-700' : 'text-gray-600'}`}>
                          {r === 'enterprise' ? 'Doanh nghiệp' : r === 'collector' ? 'Người thu gom' : 'Người dân'}
                        </span>
                      </span>
                      <input type="radio" className="hidden" name="role" value={r} checked={editRoleModal.role === r} onChange={() => setEditRoleModal({...editRoleModal, role: r})} />
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex gap-3">
                <button 
                  onClick={executeUpdateRole} 
                  disabled={isUpdatingRole} 
                  className="flex-1 py-4 bg-blue-600 hover:bg-blue-700 text-white font-black text-xs rounded-2xl shadow-xl shadow-blue-200 transition-all active:scale-95 uppercase tracking-widest"
                >
                  {isUpdatingRole ? 'ĐANG LƯU...' : 'LƯU THAY ĐỔI'}
                </button>
              </div>
            </div>
          </div>
        </Portal>
      )}
    </div>
  );
};