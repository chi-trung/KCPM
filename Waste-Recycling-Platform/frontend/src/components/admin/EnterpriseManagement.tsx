"use client";
import React, { useState, useEffect, useCallback } from 'react';
import { Table } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { CheckCircle2, XCircle, Eye, Search, Filter, AlertTriangle, Building2, MapPin, Mail, User, X } from 'lucide-react';
import { enterpriseAdminApi, EnterpriseListItem } from '@/lib/api/enterpriseAdminApi';
import { Portal } from '../shared/Portal';

export const EnterpriseManagement: React.FC = () => {
  const [enterprises, setEnterprises] = useState<EnterpriseListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'Pending' | 'Verified' | 'Rejected'>('all');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [total, setTotal] = useState(0);
  const pageSize = 10;

  const [detailModal, setDetailModal] = useState<{ isOpen: boolean; enterpriseId: string | null }>({ isOpen: false, enterpriseId: null });
  const [detailData, setDetailData] = useState<any>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [approveModal, setApproveModal] = useState<{ isOpen: boolean; enterpriseId: string | null; isReapproval?: boolean }>({ isOpen: false, enterpriseId: null });
  const [rejectModal, setRejectModal] = useState<{ isOpen: boolean; enterpriseId: string | null; reason: string }>({ isOpen: false, enterpriseId: null, reason: '' });
  
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Bộ chuyển ngữ thông minh chống sập màn hình
  const getEffectiveStatus = (row: any) => {
    if (!row) return 'Pending';
    if (row.status) {
      const s = String(row.status).toLowerCase();
      if (s === 'verified') return 'Verified';
      if (s === 'rejected') return 'Rejected';
      return 'Pending';
    }
    if (row.isVerified === true) return 'Verified';
    return 'Pending';
  };

  const fetchEnterprises = useCallback(async (silentLoad = false) => {
    if (!silentLoad) setIsLoading(true);
    setError(null);
    try {
      const isVerifiedFilter = statusFilter === 'all' ? undefined : statusFilter === 'Verified';
      const result = await enterpriseAdminApi.getEnterprises(
        page,
        pageSize,
        isVerifiedFilter,
        searchTerm || undefined
      );
      
      const data = result.data !== undefined ? result.data : result;
      setEnterprises(Array.isArray(data) ? data : []);
      setTotal(result.pagination?.total || data.length || 0);
      setTotalPages(result.pagination?.totalPages || 1);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch enterprises');
    } finally {
      if (!silentLoad) setIsLoading(false);
    }
  }, [page, pageSize, statusFilter, searchTerm]);

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      fetchEnterprises(page === 1 ? false : true);
    }, 500);
    return () => clearTimeout(timeoutId);
  }, [fetchEnterprises, page, statusFilter, searchTerm]);

  useEffect(() => {
    if (detailModal.isOpen && detailModal.enterpriseId) {
      const fetchDetail = async () => {
        setLoadingDetail(true);
        try {
          const result = await enterpriseAdminApi.getEnterpriseDetail(detailModal.enterpriseId!);
          setDetailData(result.data !== undefined ? result.data : result);
        } catch (err) {
          setError(err instanceof Error ? err.message : 'Failed to fetch enterprise detail');
        } finally {
          setLoadingDetail(false);
        }
      };
      fetchDetail();
    }
  }, [detailModal]);

  const handleApprove = async () => {
    if (!approveModal.enterpriseId) return;
    setIsSubmitting(true);
    try {
      await enterpriseAdminApi.verifyEnterprise(approveModal.enterpriseId);
      setSuccessMessage('Phê duyệt doanh nghiệp thành công');
      setApproveModal({ isOpen: false, enterpriseId: null, isReapproval: false });
      setDetailModal({ isOpen: false, enterpriseId: null });
      fetchEnterprises(true);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Lỗi khi phê duyệt');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleReject = async () => {
    if (!rejectModal.enterpriseId || !rejectModal.reason.trim()) {
      setError('Vui lòng nhập lý do từ chối');
      return;
    }
    setIsSubmitting(true);
    try {
      await enterpriseAdminApi.rejectEnterprise(rejectModal.enterpriseId, rejectModal.reason);
      setSuccessMessage('Từ chối doanh nghiệp thành công');
      setRejectModal({ isOpen: false, enterpriseId: null, reason: '' });
      setDetailModal({ isOpen: false, enterpriseId: null });
      fetchEnterprises(true);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Lỗi khi từ chối');
    } finally {
      setIsSubmitting(false);
    }
  };

  const getStatusBadgeVariant = (status: string) => {
    switch (status) {
      case 'Verified': return 'success';
      case 'Rejected': return 'danger';
      case 'Pending': return 'warning';
      default: return 'default';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'Verified': return 'Đã duyệt';
      case 'Rejected': return 'Bị từ chối';
      case 'Pending': return 'Chờ duyệt';
      default: return status || 'Không rõ';
    }
  };

  const columns = [
    { 
      key: 'companyName' as const, 
      label: 'Tên Công Ty', 
      width: '25%',
      render: (name: string) => (
        <div className="font-bold text-gray-900 text-sm">{name || 'Chưa cập nhật'}</div>
      )
    },
    { 
      key: 'status' as const, 
      label: 'Trạng Thái',
      width: '15%',
      render: (_: any, row: any) => {
        const status = getEffectiveStatus(row);
        return (
          <Badge variant={getStatusBadgeVariant(status)} className="shadow-sm font-bold text-[10px] px-2 py-0.5">
            {getStatusLabel(status)}
          </Badge>
        );
      }
    },
    { 
      key: 'serviceArea' as const, 
      label: 'Khu Vực Phục Vụ',
      width: '20%',
      render: (area?: string) => {
        if (!area) return <span className="text-gray-400">-</span>;
        try {
          const parsed = JSON.parse(area);
          const areaText = Array.isArray(parsed) ? parsed.join(', ') : parsed;
          return (
            <div className="flex items-center gap-1.5 text-gray-600 font-medium text-sm">
              <MapPin size={14} className="text-emerald-500 shrink-0" />
              <span className="truncate" title={areaText}>{areaText}</span>
            </div>
          );
        } catch (e) {
          return (
            <div className="flex items-center gap-1.5 text-gray-600 font-medium text-sm">
              <MapPin size={14} className="text-emerald-500 shrink-0" />
              <span className="truncate" title={area}>{area}</span>
            </div>
          );
        }
      }
    },
    { 
      key: 'createdAt' as const, 
      label: 'Ngày Tạo', 
      width: '15%', 
      render: (date: string) => <span className="text-gray-500 font-medium text-sm">{date ? new Date(date).toLocaleDateString('vi-VN') : '-'}</span>
    },
    {
      key: 'id' as const,
      label: 'Hành Động',
      width: '25%',
      render: (_: any, row: any) => {
        const status = getEffectiveStatus(row);
        return (
          <div className="flex gap-2 items-center">
            <Button
              size="sm"
              variant="outline"
              onClick={() => setDetailModal({ isOpen: true, enterpriseId: row.id })}
              className="gap-1 hover:bg-emerald-50 hover:text-emerald-700 hover:border-emerald-200 transition-all shadow-sm text-xs font-bold"
            >
              <Eye size={14} /> Chi tiết
            </Button>
            {status === 'Pending' && (
              <>
                <Button
                  size="sm"
                  variant="success"
                  onClick={() => setApproveModal({ isOpen: true, enterpriseId: row.id, isReapproval: false })}
                  className="shadow-sm p-1.5"
                >
                  <CheckCircle2 size={16} />
                </Button>
                <Button
                  size="sm"
                  variant="danger"
                  onClick={() => setRejectModal({ isOpen: true, enterpriseId: row.id, reason: '' })}
                  className="shadow-sm p-1.5"
                >
                  <XCircle size={16} />
                </Button>
              </>
            )}
          </div>
        );
      }
    },
  ];

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pt-2">
      {/* Alerts */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex gap-3 items-center animate-in slide-in-from-top-2">
          <AlertTriangle className="text-red-600 shrink-0" size={18} />
          <p className="text-red-700 text-sm font-medium">{error}</p>
          <button onClick={() => setError(null)} className="ml-auto text-red-400 hover:text-red-600">✕</button>
        </div>
      )}
      {successMessage && (
        <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4 flex gap-3 items-center animate-in slide-in-from-top-2">
          <CheckCircle2 className="text-emerald-600 shrink-0" size={18} />
          <p className="text-emerald-800 font-bold text-sm">{successMessage}</p>
          <button onClick={() => setSuccessMessage(null)} className="ml-auto text-emerald-400 hover:text-emerald-600">✕</button>
        </div>
      )}

      {/* Filters Card */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
        <div className="flex flex-col sm:flex-row gap-4 items-end">
          <div className="flex-1 w-full relative">
            <label htmlFor="ent-mgmt-search" className="block text-sm font-semibold text-gray-700 mb-2">Tìm kiếm doanh nghiệp</label>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={18} />
              <input
                id="ent-mgmt-search"
                type="text"
                placeholder="Nhập tên công ty cần tìm..."
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setPage(1);
                }}
                className="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-500 transition-all text-sm font-medium"
              />
            </div>
          </div>

          <div className="w-full sm:w-64">
            <label htmlFor="ent-mgmt-status" className="block text-sm font-semibold text-gray-700 mb-2">Trạng thái</label>
            <div className="relative">
              <select
                id="ent-mgmt-status"
                value={statusFilter}
                onChange={(e) => {
                  setStatusFilter(e.target.value as any);
                  setPage(1);
                }}
                className="w-full px-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-500 appearance-none bg-white cursor-pointer transition-all text-sm font-bold text-gray-700"
              >
                <option value="all">Tất cả trạng thái</option>
                <option value="Pending">Chờ duyệt</option>
                <option value="Verified">Đã duyệt</option>
                <option value="Rejected">Bị từ chối</option>
              </select>
              <Filter className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" size={16} />
            </div>
          </div>
        </div>
      </div>

      {/* Table Card */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
        {isLoading ? (
          <div className="p-16 flex flex-col items-center justify-center">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-gray-100 border-t-emerald-600"></div>
            <p className="mt-4 text-gray-500 font-bold text-sm">Đang đồng bộ dữ liệu...</p>
          </div>
        ) : enterprises.length === 0 ? (
          <div className="p-16 text-center">
            <div className="bg-gray-50 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4 border border-gray-100">
              <Building2 size={24} className="text-gray-400" />
            </div>
            <p className="text-gray-900 font-bold">Trống</p>
            <p className="text-gray-500 text-sm mt-1 font-medium">Không tìm thấy doanh nghiệp nào.</p>
          </div>
        ) : (
          <Table columns={columns} data={enterprises} />
        )}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-between items-center px-2">
          <p className="text-gray-500 text-xs font-bold uppercase tracking-wider">
            Hiển thị <span className="text-gray-900">{(page - 1) * pageSize + 1} - {Math.min(page * pageSize, total)}</span> / {total}
          </p>
          <div className="flex items-center gap-2 bg-white p-1 rounded-xl shadow-sm border border-gray-100">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage(Math.max(1, page - 1))}
              disabled={page === 1}
              className="border-none hover:bg-gray-100 text-xs font-bold px-4"
            >
              Trước
            </Button>
            <div className="px-3 py-1 bg-emerald-50 rounded-lg text-xs font-extrabold text-emerald-700">
              {page} / {totalPages}
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage(Math.min(totalPages, page + 1))}
              disabled={page === totalPages}
              className="border-none hover:bg-gray-100 text-xs font-bold px-4"
            >
              Tiếp
            </Button>
          </div>
        </div>
      )}

      {/* Detail Modal */}
      {detailModal.isOpen && (
        <Portal>
          <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-50 animate-in fade-in duration-200">
            <div className="bg-white rounded-2xl max-w-2xl w-full max-h-[90vh] flex flex-col shadow-2xl m-4 animate-in zoom-in-95 duration-200">
              <div className="p-6 border-b border-gray-100 flex justify-between items-center bg-gray-50/50 rounded-t-2xl shrink-0">
                <div className="flex items-center gap-3">
                  <div className="bg-emerald-100 p-2 rounded-lg text-emerald-600 shadow-sm border border-emerald-200">
                    <Building2 size={20} />
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-gray-900">Hồ Sơ Doanh Nghiệp</h3>
                    <p className="text-xs font-bold text-gray-500 mt-0.5">Mã hồ sơ: #{detailModal.enterpriseId?.split('-')[0].toUpperCase()}</p>
                  </div>
                </div>
                <button
                  onClick={() => setDetailModal({ isOpen: false, enterpriseId: null })}
                  className="p-2 text-gray-400 hover:bg-white hover:text-gray-700 rounded-full transition-all border border-transparent hover:border-gray-200 hover:shadow-sm"
                >
                  <X size={20} />
                </button>
              </div>

              <div className="p-6 overflow-y-auto grow space-y-6">
                {loadingDetail ? (
                  <div className="py-20 flex flex-col items-center justify-center">
                    <div className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-gray-100 border-t-emerald-600"></div>
                    <p className="mt-4 text-gray-500 font-bold text-sm">Đang tải hồ sơ...</p>
                  </div>
                ) : detailData && (
                  <>
                    <div className="bg-emerald-50/50 border border-emerald-100 p-5 rounded-xl flex items-start justify-between">
                      <div>
                        <p className="text-[10px] font-extrabold tracking-widest text-emerald-800 uppercase mb-1">Tên tổ chức / Công ty</p>
                        <p className="text-xl font-bold text-gray-900 leading-tight">{detailData.companyName || 'Chưa cập nhật'}</p>
                        <div className="flex flex-wrap gap-4 mt-4 text-xs font-bold text-gray-600">
                          <span className="flex items-center gap-1.5 bg-white px-2 py-1 rounded-md shadow-sm border border-gray-100"><Mail size={14} className="text-emerald-500"/> {detailData.userEmail || 'Chưa cập nhật'}</span>
                          <span className="flex items-center gap-1.5 bg-white px-2 py-1 rounded-md shadow-sm border border-gray-100"><User size={14} className="text-emerald-500"/> {detailData.userFullName || 'Chưa cập nhật'}</span>
                        </div>
                      </div>
                      <Badge variant={getStatusBadgeVariant(getEffectiveStatus(detailData))} className="px-3 py-1 font-extrabold text-[10px] shadow-sm">
                        {getStatusLabel(getEffectiveStatus(detailData)).toUpperCase()}
                      </Badge>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div className="bg-white border border-gray-100 p-4 rounded-xl shadow-sm">
                        <span className="flex items-center gap-1.5 text-xs font-extrabold text-gray-400 uppercase tracking-wider mb-2">
                          <MapPin size={14} className="text-emerald-500"/> Khu vực hoạt động
                        </span>
                        <p className="font-bold text-gray-900 text-sm">{detailData.serviceArea || 'Không xác định'}</p>
                      </div>

                      <div className="bg-white border border-gray-100 p-4 rounded-xl shadow-sm">
                        <span className="text-xs font-extrabold text-gray-400 uppercase tracking-wider mb-2 block">Năng lực xử lý</span>
                        <p className="font-extrabold text-emerald-700 text-sm">
                          {detailData.capacityKgPerDay ? `${detailData.capacityKgPerDay.toLocaleString()} kg/ngày` : 'Chưa xác định'}
                        </p>
                      </div>

                      <div className="bg-white border border-gray-100 p-4 rounded-xl shadow-sm">
                        <span className="text-xs font-extrabold text-gray-400 uppercase tracking-wider mb-2 block">Nhân sự hiện tại</span>
                        <p className="font-bold text-gray-900 text-sm">{detailData.collectorCount || 0} <span className="text-[10px] text-gray-500">thành viên</span></p>
                      </div>

                      <div className="bg-white border border-gray-100 p-4 rounded-xl shadow-sm">
                        <span className="text-xs font-extrabold text-gray-400 uppercase tracking-wider mb-2 block">Loại rác thu gom</span>
                        <p className="font-bold text-gray-900 text-sm">{detailData.wasteTypeCount || 0} <span className="text-[10px] text-gray-500">nhóm rác</span></p>
                      </div>
                    </div>

                    {detailData.rejectionReason && (
                      <div className="bg-red-50 border border-red-100 rounded-xl p-4">
                        <span className="flex items-center gap-2 text-xs font-extrabold text-red-700 uppercase tracking-wider mb-2">
                          <AlertTriangle size={14}/> Lịch sử từ chối
                        </span>
                        <p className="text-sm font-medium text-red-800 leading-relaxed italic">"{detailData.rejectionReason}"</p>
                      </div>
                    )}
                  </>
                )}
              </div>

              <div className="border-t border-gray-100 p-6 flex justify-end gap-3 bg-gray-50/50 rounded-b-2xl shrink-0">
                <Button
                  variant="outline"
                  className="px-6 rounded-xl hover:bg-white border-gray-300 font-bold text-xs shadow-sm transition-all"
                  onClick={() => setDetailModal({ isOpen: false, enterpriseId: null })}
                >
                  ĐÓNG
                </Button>
                {getEffectiveStatus(detailData) === 'Pending' && (
                  <>
                    <Button
                      variant="danger"
                      className="px-6 rounded-xl font-bold text-xs shadow-sm hover:scale-105 transition-all"
                      onClick={() => setRejectModal({ isOpen: true, enterpriseId: detailData.id, reason: '' })}
                    >
                      TỪ CHỐI
                    </Button>
                    <Button
                      variant="success"
                      className="px-8 rounded-xl bg-emerald-600 hover:bg-emerald-700 font-extrabold text-xs shadow-md shadow-emerald-200 hover:scale-105 transition-all"
                      onClick={() => setApproveModal({ isOpen: true, enterpriseId: detailData.id, isReapproval: false })}
                    >
                      DUYỆT ĐỐI TÁC
                    </Button>
                  </>
                )}
                {getEffectiveStatus(detailData) === 'Verified' && (
                  <Button
                    variant="danger"
                    className="px-6 rounded-xl font-bold text-xs shadow-sm"
                    onClick={() => setRejectModal({ isOpen: true, enterpriseId: detailData.id, reason: '' })}
                  >
                    HUỶ DUYỆT
                  </Button>
                )}
                {getEffectiveStatus(detailData) === 'Rejected' && (
                  <Button
                    variant="success"
                    className="px-8 rounded-xl bg-emerald-600 font-extrabold text-xs shadow-md"
                    onClick={() => setApproveModal({ isOpen: true, enterpriseId: detailData.id, isReapproval: true })}
                  >
                    PHÊ DUYỆT LẠI
                  </Button>
                )}
              </div>
            </div>
          </div>
        </Portal>
      )}

      {/* Approve Confirm Modal */}
      {approveModal.isOpen && (
        <Portal>
          <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-[60] animate-in fade-in duration-200">
            <div className="bg-white rounded-2xl p-8 max-w-sm w-full mx-4 shadow-2xl animate-in zoom-in-95 duration-200 text-center">
              <div className="bg-emerald-100 w-16 h-16 rounded-full flex items-center justify-center mb-6 mx-auto shadow-inner border-4 border-white">
                <CheckCircle2 className="text-emerald-600" size={32} />
              </div>
              <h3 className="text-xl font-bold text-gray-900 mb-2">
                {approveModal.isReapproval ? 'Phê duyệt lại?' : 'Xác nhận phê duyệt'}
              </h3>
              <p className="text-gray-500 text-sm font-medium mb-8 leading-relaxed">
                Doanh nghiệp sẽ được quyền truy cập vào các tác vụ thu gom của hệ thống ngay lập tức.
              </p>
              <div className="flex gap-3">
                <Button
                  variant="outline"
                  className="flex-1 rounded-xl font-bold text-xs py-3 border-gray-300"
                  onClick={() => setApproveModal({ isOpen: false, enterpriseId: null, isReapproval: false })}
                  disabled={isSubmitting}
                >
                  HUỶ
                </Button>
                <Button
                  variant="success"
                  className="flex-1 rounded-xl bg-emerald-600 hover:bg-emerald-700 font-extrabold text-xs py-3 shadow-md shadow-emerald-200"
                  isLoading={isSubmitting}
                  onClick={handleApprove}
                >
                  {isSubmitting ? 'ĐANG XỬ LÝ' : 'XÁC NHẬN'}
                </Button>
              </div>
            </div>
          </div>
        </Portal>
      )}

      {/* Reject Reason Modal */}
      {rejectModal.isOpen && (
        <Portal>
          <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-[60] animate-in fade-in duration-200">
            <div className="bg-white rounded-2xl p-6 max-w-sm w-full mx-4 shadow-2xl animate-in zoom-in-95 duration-200">
              <div className="flex items-center gap-3 mb-6">
                <div className="bg-red-100 p-2 rounded-lg text-red-600 border border-red-200 shadow-sm">
                  <XCircle size={20} />
                </div>
                <h3 className="text-lg font-bold text-gray-900 uppercase tracking-tight">Lý do từ chối</h3>
              </div>
              
              <textarea
                placeholder="Ví dụ: Thông tin giấy phép kinh doanh không khớp, thiếu hồ sơ nhân sự..."
                value={rejectModal.reason}
                onChange={(e) => setRejectModal({ ...rejectModal, reason: e.target.value })}
                className="w-full p-4 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-red-500 mb-6 bg-gray-50 text-sm font-medium leading-relaxed resize-none transition-all placeholder:text-gray-400"
                rows={4}
              />
              
              <div className="flex gap-3">
                <Button
                  variant="outline"
                  className="flex-1 rounded-xl font-bold text-xs py-3"
                  onClick={() => setRejectModal({ isOpen: false, enterpriseId: null, reason: '' })}
                  disabled={isSubmitting}
                >
                  QUAY LẠI
                </Button>
                <Button
                  variant="danger"
                  className="flex-1 rounded-xl font-extrabold text-xs py-3 shadow-md shadow-red-200"
                  isLoading={isSubmitting}
                  onClick={handleReject}
                >
                  XÁC NHẬN TỪ CHỐI
                </Button>
              </div>
            </div>
          </div>
        </Portal>
      )}
    </div>
  );
};