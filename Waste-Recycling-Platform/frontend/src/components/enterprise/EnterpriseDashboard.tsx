"use client";
import { useState, useEffect } from "react";
import {
  LayoutDashboard,
  ClipboardList,
  Factory,
  Trophy,
  CheckSquare,
  Users,
  ChartColumnBig,
  Settings,
  History,
} from "lucide-react";
import { reportApi } from "../../lib/api/reportApi";
import {
  enterpriseTaskApi,
  EnterpriseCollector,
  EnterpriseProfile,
  EnterpriseTaskStats,
  EnterpriseWasteCategory,
} from "../../lib/api/enterpriseTaskApi";
import {
  enterpriseRewardApi,
  EnterpriseRewardRule,
  UpdateEnterpriseRewardRuleItem,
} from "../../lib/api/enterpriseRewardApi";
import { useAuth } from "@/contexts/AuthContext";
import { useSignalR } from "@/hooks/useSignalR";
import { EnterpriseOverview } from "./EnterpriseOverview";
import { RequestManagement } from "./RequestManagement";
import { CapacitySettings } from "./CapacitySettings";
import { RewardConfiguration } from "./RewardConfiguration";
import { EnterpriseTaskManagement } from "./EnterpriseTaskManagement";
import { CollectorsManagement } from "./CollectorsManagement";
import { ReportsAnalytics } from "./ReportsAnalytics";
import { EnterpriseWasteAnalytics } from "./EnterpriseWasteAnalytics";
import { ProfileSettings } from "./ProfileSettings";
import { EnterpriseHistoryTable } from "./EnterpriseHistoryTable";
import { EnterpriseRequest } from "./types";

interface EnterpriseDashboardProps {
  initialTab?: string;
}

export const EnterpriseDashboard: React.FC<EnterpriseDashboardProps> = ({ initialTab = "dashboard" }) => {
  const { user, logout } = useAuth();
  const [activeTab, setActiveTab] = useState(initialTab);
  const [requests, setRequests] = useState<EnterpriseRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [profileLoading, setProfileLoading] = useState(true);
  const [profileError, setProfileError] = useState<string | null>(null);
  const [enterpriseProfile, setEnterpriseProfile] = useState<EnterpriseProfile>({
    id: "",
    companyName: "",
    serviceArea: "",
    capacityKgPerDay: null,
    status: "Pending", // BƯỚC 1: Thêm Status
    rejectionReason: undefined, // BƯỚC 1: Thêm Rejection Reason
  });
  const [categories, setCategories] = useState<EnterpriseWasteCategory[]>([]);
  const [acceptedWasteTypeIds, setAcceptedWasteTypeIds] = useState<number[]>([]);
  const [collectors, setCollectors] = useState<EnterpriseCollector[]>([]);
  const [taskStats, setTaskStats] = useState<EnterpriseTaskStats | null>(null);
  const [rewardRules, setRewardRules] = useState<EnterpriseRewardRule[]>([]);
  const [rewardLoading, setRewardLoading] = useState(true);
  const [rewardError, setRewardError] = useState<string | null>(null);
  const [complaintsData, setComplaintsData] = useState<any>(null);
  const [complaintsLoading, setComplaintsLoading] = useState(false);
  const [selectedComplaint, setSelectedComplaint] = useState<any>(null);
  const [responseText, setResponseText] = useState("");
  const [respondingComplaintId, setRespondingComplaintId] = useState<string | null>(null);

  // Capacity State for overview card
  const [capacity, setCapacity] = useState({
    wasteTypes: ["plastic", "paper"],
    maxCapacity: 5000,
    serviceArea: "HCMC",
  });

  const fetchReports = async () => {
    setLoading(true);
    setError(null);
    try {
        const response = await reportApi.getEnterpriseAvailableReports(1, 10, "Pending");
        const transformedRequests: EnterpriseRequest[] = response.reports.map((report: any) => ({
          reportId: report.id,
          type: report.categoryName || "Unknown",
          quantity: "N/A",
          location: report.address || "Unknown",
          status: report.status || "Pending",
          date: new Date(report.createdAt).toLocaleDateString("vi-VN"),
          requester: report.citizenName || "Unknown",
        }));
        setRequests(transformedRequests);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to fetch reports");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    const fetchEnterpriseProfile = async () => {
      setProfileLoading(true);
      setProfileError(null);
      try {
        // BƯỚC 1: LUÔN gọi getProfile() TRƯỚC để lấy trạng thái
        const profileResponse = await enterpriseTaskApi.getProfile();

        // Lưu profile kèm theo status + rejectionReason
        setEnterpriseProfile({
          id: profileResponse.id,
          companyName: profileResponse.companyName,
          serviceArea: profileResponse.serviceArea ?? "",
          capacityKgPerDay: profileResponse.capacityKgPerDay,
          status: profileResponse.status ?? "Pending", // Lấy status từ API
          rejectionReason: profileResponse.rejectionReason, // Lấy rejection reason từ API
        });

        // BƯỚC 2: LUÔN LUÔN gọi getWasteTypes (để form CapacitySettings hiển thị danh sách)
        const wasteTypesResponse = await enterpriseTaskApi.getWasteTypes().catch((error) => {
          console.error("🚨 BẮT ĐƯỢC LỖI GỌI API RÁC:", error);
          return { allCategories: [], acceptedIds: [] };
        });
        
        console.log("📦 DỮ LIỆU RÁC TRẢ VỀ:", wasteTypesResponse);
        
        // 👇 SET DỮ LIỆU CHO FORM CẬP NHẬT CÔNG SUẤT 👇
        setCategories(wasteTypesResponse.allCategories || []);
        setAcceptedWasteTypeIds(wasteTypesResponse.acceptedIds || []);
        setCapacity({
          wasteTypes: (wasteTypesResponse.allCategories || [])
            .filter((category: any) => ((wasteTypesResponse.acceptedIds as any[]) || []).includes(category.id))
            .map((category: any) => category.name),
          maxCapacity: profileResponse.capacityKgPerDay ?? 0,
          serviceArea: profileResponse.serviceArea ?? "",
        });

        // BƯỚC 3: CHỈ KHI ĐÃ VERIFIED mới gọi các API Thống kê, Phần thưởng, Nhân viên
        if (profileResponse.status === "Verified") {
          const [rewardRulesResponse, statsResponse, collectorsResponse] = await Promise.all([
            enterpriseRewardApi.getRewardRules().catch(() => []),
            enterpriseTaskApi.getStats().catch(() => null),
            enterpriseTaskApi.getAvailableCollectors().catch(() => []),
          ]);

          setRewardRules(rewardRulesResponse);
          setTaskStats(statsResponse);
          setCollectors(collectorsResponse);
        }
      } catch (err) {
        setProfileError(err instanceof Error ? err.message : "Failed to load enterprise profile");
        console.error(err);
      } finally {
        setProfileLoading(false);
        setRewardLoading(false);
      }
    };

  const fetchTaskStats = async () => {
    try {
      const statsResponse = await enterpriseTaskApi.getStats();
      setTaskStats(statsResponse);
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchReports();
    fetchEnterpriseProfile();
    fetchTaskStats();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (activeTab === "complaints") {
      fetchComplaints();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab]);

  const fetchComplaints = async () => {
    setComplaintsLoading(true);
    try {
      const token = localStorage.getItem("token");
      const baseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080/api';
      const res = await fetch(`${baseUrl}/enterprise/tasks/complaints?page=1&pageSize=20`, {
        headers: { Authorization: token ? `Bearer ${token}` : "" },
      });
      if (res.ok) {
        const json = await res.json();
        setComplaintsData(json.data || []);
      } else {
        console.error('Failed to load complaints', res.status);
        setComplaintsData([]);
      }
    } catch (err) {
      console.error('Error fetching complaints:', err);
      setComplaintsData([]);
    } finally {
      setComplaintsLoading(false);
    }
  };

  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

  const respondToComplaint = async (complaintId: string, response: string, resolveImmediately: boolean = false, escalateToAdmin: boolean = false) => {
    // Only require text for respond and escalate, not for resolve
    if (!resolveImmediately && (!response || !response.trim())) {
      alert("Vui lòng nhập nội dung phản hồi");
      return;
    }
    
    // Confirm dialog for resolve
    if (resolveImmediately) {
      const confirmed = confirm("Bạn có chắc chắn muốn đóng khiếu nại này?\n\nKhiếu nại sẽ được đánh dấu là đã giải quyết và Citizen sẽ nhận được thông báo.\n\nLưu ý: Không cần nhập phản hồi nếu đã xử lý xong.");
      if (!confirmed) return;
    }
    
    try {
      const baseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080/api';
      const res = await fetch(`${baseUrl}/enterprise/tasks/complaints/${complaintId}/respond`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: token ? `Bearer ${token}` : "",
        },
        body: JSON.stringify({
          response,
          resolveImmediately,
          escalateToAdmin,
        }),
      });

      if (res.ok) {
        const json = await res.json();
        alert(json.message);
        // Refresh complaints list
        fetchComplaints();
        // Clear form
        setResponseText("");
        setRespondingComplaintId(null);
        setSelectedComplaint(null);
      } else {
        const error = await res.json();
        alert(error.message || "Failed to respond to complaint");
      }
    } catch (err) {
      console.error('Error responding to complaint:', err);
      alert("Error responding to complaint");
    }
  };

  // SignalR: notify enterprise of resolved complaints (for completeness)
  useSignalR({
    enabled: !!user,
    token,
    onComplaintResolved: (complaintId, message, adminResponse) => {
      // push a simple notification to the UI
      setComplaintsData((prev: any) => {
        // preserve previous data and also append a small notification entry
        return prev;
      });
      // simple browser alert for testing
      try {
        // eslint-disable-next-line no-alert
        alert(`Complaint ${complaintId} resolved: ${message}`);
      } catch {}
    },
  });

  const refreshCollectors = async () => {
    try {
      const latestCollectors = await enterpriseTaskApi.getAvailableCollectors();
      setCollectors(latestCollectors);
    } catch (err) {
      console.error(err);
    }
  };

  const refreshStats = async () => {
    try {
      const stats = await enterpriseTaskApi.getStats();
      setTaskStats(stats);
    } catch (err) {
      console.error(err);
    }
  };

  const handleStatusChange = (reportId: string, status: string) => {
    setRequests((prev) => prev.map((req) => (req.reportId === reportId ? { ...req, status } : req)));
  };

  const handleAssign = (reportId: string, collectorId: string) => {
    handleStatusChange(reportId, "Assigned");
    alert(`Task assigned to collector ${collectorId}`);
    refreshStats();
    refreshCollectors();
  };

  const handleSaveCapacity = async (payload: {
    serviceArea: string;
    capacityKgPerDay: number | null;
    wasteCategoryIds: number[];
  }) => {
    setProfileLoading(true);
    setProfileError(null);
    try {
      await enterpriseTaskApi.updateProfile({
        serviceArea: payload.serviceArea,
        capacityKgPerDay: payload.capacityKgPerDay,
      });
      await enterpriseTaskApi.updateWasteTypes({ wasteCategoryIds: payload.wasteCategoryIds });

      setEnterpriseProfile((prev) => ({
        ...prev,
        serviceArea: payload.serviceArea,
        capacityKgPerDay: payload.capacityKgPerDay,
      }));
      setAcceptedWasteTypeIds(payload.wasteCategoryIds);
      setCapacity({
        wasteTypes: categories
          .filter((category) => payload.wasteCategoryIds.includes(category.id))
          .map((category) => category.name),
        maxCapacity: payload.capacityKgPerDay ?? 0,
        serviceArea: payload.serviceArea,
      });
      alert("Enterprise profile updated successfully.");
    } catch (err) {
      setProfileError(err instanceof Error ? err.message : "Failed to save enterprise settings");
      console.error(err);
      alert(profileError || "Failed to save enterprise settings.");
    } finally {
      setProfileLoading(false);
    }
  };

  const handleSaveRewardRules = async (rules: UpdateEnterpriseRewardRuleItem[]) => {
    setRewardLoading(true);
    setRewardError(null);

    try {
      await enterpriseRewardApi.updateRewardRules(rules);
      const latestRules = await enterpriseRewardApi.getRewardRules();
      setRewardRules(latestRules);
      alert("Reward rules updated successfully.");
    } catch (err) {
      const message = err instanceof Error ? err.message : "Failed to update reward rules";
      setRewardError(message);
      console.error(err);
      alert(message);
    } finally {
      setRewardLoading(false);
    }
  };

  const tabs = [
    {
      id: "dashboard",
      label: "Dashboard",
      icon: LayoutDashboard,
      description: "Overview of requests, capacity, and operations",
    },
    {
      id: "requests",
      label: "Collection Requests",
      icon: ClipboardList,
      description: "Review and approve incoming waste reports",
    },
    {
      id: "tasks",
      label: "Assign Tasks",
      icon: CheckSquare,
      description: "Assign approved requests to collectors",
    },
    {
      id: "complaints",
      label: "Complaints",
      icon: ClipboardList,
      description: "View and manage citizen complaints",
    },
    {
      id: "history",
      label: "History",
      icon: History,
      description: "View completed task history",
    },
    {
      id: "collectors",
      label: "Collectors",
      icon: Users,
      description: "Monitor collector availability and workload",
    },
    {
      id: "capacity",
      label: "Capacity Management",
      icon: Factory,
      description: "Configure service area, categories, and capacity",
    },
    {
      id: "analytics",
      label: "Reports & Analytics",
      icon: ChartColumnBig,
      description: "Track waste statistics by area, type, and time trends",
    },
    {
      id: "rewards",
      label: "Reward Rules",
      icon: Trophy,
      description: "Set points and quality bonus by waste category",
    },
    {
      id: "settings",
      label: "Profile / Settings",
      icon: Settings,
      description: "Enterprise profile and account controls",
    },
  ];

  const activeConfig = tabs.find((tab) => tab.id === activeTab) ?? tabs[0];

  // ========== BƯỚC 3: GUARD LOGIC (TRẠM GÁC) ==========
  
  // Guard 1: Nếu đang load profile → Hiện loading
  if (profileLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center space-y-4">
          <div className="inline-block animate-spin rounded-full h-10 w-10 border-4 border-gray-200 border-t-emerald-600"></div>
          <p className="text-gray-600 font-medium">Đang tải hồ sơ doanh nghiệp...</p>
        </div>
      </div>
    );
  }

  // Guard 2: Nếu chưa điền thông tin HOẶC bị từ chối → Buộc hiện form
  if (!enterpriseProfile.capacityKgPerDay || !enterpriseProfile.serviceArea || enterpriseProfile.status === "Rejected") {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 p-6">
        <div className="max-w-3xl mx-auto">
          {/* Tiêu đề */}
          <div className="mb-6">
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Hoàn Thành Hồ Sơ Doanh Nghiệp</h1>
            <p className="text-gray-600">Để bắt đầu nhận nhiệm vụ thu gom, vui lòng điền đầy đủ thông tin công suất và khu vực phục vụ.</p>
          </div>

          {/* Warning nếu bị reject */}
          {enterpriseProfile.status === "Rejected" && enterpriseProfile.rejectionReason && (
            <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-6 rounded">
              <h3 className="font-bold text-red-900 mb-2">❌ Hồ Sơ Bị Từ Chối</h3>
              <p className="text-red-800 text-sm"><strong>Lý do:</strong> {enterpriseProfile.rejectionReason}</p>
              <p className="text-red-700 text-sm mt-2">Vui lòng cập nhật thông tin và gửi lại để Admin xem xét.</p>
            </div>
          )}

          {/* Form nhập thông tin */}
          <CapacitySettings
            profile={enterpriseProfile}
            categories={categories}
            acceptedIds={acceptedWasteTypeIds}
            onSave={handleSaveCapacity}
            saving={profileLoading}
            error={profileError}
          />
        </div>
      </div>
    );
  }

  // Guard 3: Nếu đã nhập nhưng đang chờ Admin duyệt → Hiện màn hình chờ
  if (enterpriseProfile.status === "Pending") {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-blue-100 flex items-center justify-center p-6">
        <div className="max-w-md w-full bg-white rounded-2xl shadow-xl p-8 text-center">
          <div className="mb-6 flex justify-center">
            <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-blue-100 animated-bounce">
              <svg className="w-8 h-8 text-blue-600 animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            </div>
          </div>
          
          <h2 className="text-2xl font-bold text-gray-900 mb-3">Chờ Xác Nhận Admin</h2>
          
          <p className="text-gray-600 mb-6 leading-relaxed">
            Hồ sơ doanh nghiệp của bạn đã được gửi đến Admin để xem xét. 
            <br />
            <strong>Quá trình phê duyệt thường mất 24-48 giờ.</strong>
          </p>

          <div className="bg-blue-50 rounded-lg p-4 mb-6 text-left">
            <p className="text-sm text-blue-900"><strong>📋 Thông tin hồ sơ:</strong></p>
            <ul className="text-sm text-blue-800 mt-2 space-y-1">
              <li>• <strong>Công ty:</strong> {enterpriseProfile.companyName}</li>
              <li>• <strong>Khu vực:</strong> {enterpriseProfile.serviceArea}</li>
              <li>• <strong>Công suất:</strong> {enterpriseProfile.capacityKgPerDay?.toLocaleString()} kg/ngày</li>
            </ul>
          </div>

          <div className="text-center text-sm text-gray-500">
            <p>Bạn sẽ nhận được thông báo khi Admin xác nhận.</p>
            <p className="mt-2">Vui lòng kiểm tra email của bạn định kỳ.</p>
          </div>

          <button
            onClick={() => window.location.reload()}
            className="mt-6 w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition font-medium"
          >
            Làm Mới Trang
          </button>
        </div>
      </div>
    );
  }

  // ========== Guard Passed! Hiện Dashboard Bình Thường ==========
  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-[260px_1fr]">
      <aside className="rounded-2xl border border-gray-200 bg-white p-4 shadow-sm lg:sticky lg:top-4 lg:h-fit">
        <div className="mb-4 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-500 p-4 text-white">
          <p className="text-xs uppercase tracking-wide text-emerald-100">Recycling Enterprise</p>
          <p className="mt-1 text-lg font-semibold">{enterpriseProfile.companyName || user?.fullName || "Enterprise"}</p>
          <p className="text-xs text-emerald-100">Operations Center</p>
        </div>

        <nav className="space-y-1" aria-label="Enterprise Sections">
          {tabs.map((tab) => {
            const Icon = tab.icon;
            const isActive = tab.id === activeTab;

            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm transition-colors ${
                  isActive
                    ? "bg-emerald-50 text-emerald-700"
                    : "text-gray-600 hover:bg-gray-50 hover:text-gray-900"
                }`}
              >
                <Icon size={18} className={isActive ? "text-emerald-600" : "text-gray-400"} />
                <span className="font-medium">{tab.label}</span>
              </button>
            );
          })}
        </nav>
      </aside>

      <section className="space-y-6">
        <header className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">{activeConfig.label}</h2>
              <p className="mt-1 text-sm text-gray-600">{activeConfig.description}</p>
            </div>
            <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
              <div className="rounded-lg bg-gray-50 px-3 py-2 text-center">
                <p className="text-xs text-gray-500">Pending</p>
                <p className="text-lg font-semibold text-amber-600">
                  {requests.filter((request) => request.status === "PENDING").length}
                </p>
              </div>
              <div className="rounded-lg bg-gray-50 px-3 py-2 text-center">
                <p className="text-xs text-gray-500">Collectors</p>
                <p className="text-lg font-semibold text-sky-700">{collectors.length}</p>
              </div>
              <div className="rounded-lg bg-gray-50 px-3 py-2 text-center col-span-2 sm:col-span-1">
                <p className="text-xs text-gray-500">Collected</p>
                <p className="text-lg font-semibold text-emerald-700">{taskStats?.totalCollected ?? 0}</p>
              </div>
            </div>
          </div>
        </header>

        {loading && activeTab === "requests" && (
          <div className="text-center py-8">
            <p className="text-gray-600">Loading reports...</p>
          </div>
        )}

        {error && activeTab === "requests" && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <p className="text-red-700">{error}</p>
          </div>
        )}
        {activeTab === "dashboard" && taskStats && <EnterpriseOverview capacity={capacity} requests={requests} stats={taskStats} />}

        {activeTab === "requests" && (
          <RequestManagement
            requests={requests}
            onStatusChange={handleStatusChange}
            onAssign={handleAssign}
          />
        )}

        {activeTab === "tasks" && <EnterpriseTaskManagement />}

        {activeTab === "history" && <EnterpriseHistoryTable />}

        {activeTab === "complaints" && (
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold">Khiếu nại từ người dân</h3>
              <button
                className="bg-emerald-600 text-white px-4 py-2 rounded-lg hover:bg-emerald-700"
                onClick={() => fetchComplaints()}
                disabled={complaintsLoading}
              >
                {complaintsLoading ? "Đang tải..." : "Tải lại"}
              </button>
            </div>
            <div>
              {complaintsLoading && <div className="text-center py-8 text-gray-600">Đang tải khiếu nại...</div>}
              {!complaintsLoading && (!complaintsData || complaintsData.length === 0) && (
                <div className="text-center py-8 text-gray-500">Chưa có khiếu nại nào</div>
              )}
              {!complaintsLoading && complaintsData && complaintsData.length > 0 && (
                <div className="space-y-3">
                  {complaintsData.map((c: any) => (
                    <div key={c.id} className="border p-4 rounded-lg bg-white shadow-sm">
                      <div className="flex justify-between items-start mb-2">
                        <span className="font-medium">{c.citizenName || "Unknown"}</span>
                        <span className={`text-xs px-2 py-1 rounded ${
                          c.status === "Open" ? "bg-blue-100 text-blue-800" :
                          c.status === "InProgress" ? "bg-yellow-100 text-yellow-800" :
                          c.status === "Resolved" ? "bg-green-100 text-green-800" :
                          c.status === "Escalated" ? "bg-red-100 text-red-800" :
                          "bg-gray-100 text-gray-800"
                        }`}>{c.status}</span>
                      </div>
                      <div className="text-gray-700 mb-2">{c.content}</div>
                      <div className="text-xs text-gray-500 mb-3">
                        {new Date(c.createdAt).toLocaleDateString("vi-VN")}
                      </div>
                      {(c.status === "Open" || c.status === "InProgress") && (
                        <div className="mt-3 pt-3 border-t">
                          {respondingComplaintId === c.id ? (
                            <div className="space-y-2">
                              <textarea
                                className="w-full border rounded-lg p-2 text-sm"
                                rows={3}
                                placeholder="Nhập phản hồi..."
                                value={responseText}
                                onChange={(e) => setResponseText(e.target.value)}
                              />
                              <div className="flex gap-2 flex-wrap">
                                <button
                                  className="bg-emerald-600 text-white px-3 py-1 rounded text-sm"
                                  onClick={() => respondToComplaint(c.id, responseText, true, false)}
                                >
                                  Đóng khiếu nại
                                </button>
                                <button
                                  className="bg-blue-600 text-white px-3 py-1 rounded text-sm"
                                  onClick={() => respondToComplaint(c.id, responseText, false, false)}
                                  disabled={!responseText.trim()}
                                >
                                  Phản hồi
                                </button>
                                <button
                                  className="bg-red-600 text-white px-3 py-1 rounded text-sm"
                                  onClick={() => respondToComplaint(c.id, responseText, false, true)}
                                  disabled={!responseText.trim()}
                                >
                                  Chuyển Admin
                                </button>
                                <button
                                  className="bg-gray-300 text-gray-700 px-3 py-1 rounded text-sm"
                                  onClick={() => { setRespondingComplaintId(null); setResponseText(""); }}
                                >
                                  Hủy
                                </button>
                              </div>
                            </div>
                          ) : (
                            <button
                              className="text-emerald-600 text-sm font-medium"
                              onClick={() => setRespondingComplaintId(c.id)}
                            >
                              Phản hồi
                            </button>
                          )}
                        </div>
                      )}
                      {c.enterpriseResponse && (
                        <div className="mt-3 p-2 bg-gray-50 rounded text-sm">
                          <span className="font-medium">Phản hồi:</span>
                          <p className="text-gray-700 mt-1">{c.enterpriseResponse}</p>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}

        {activeTab === "capacity" && (
          <CapacitySettings
            profile={enterpriseProfile}
            categories={categories}
            acceptedIds={acceptedWasteTypeIds}
            onSave={handleSaveCapacity}
            saving={profileLoading}
            error={profileError}
          />
        )}

        {activeTab === "analytics" && <EnterpriseWasteAnalytics />}

        {activeTab === "rewards" && (
          <RewardConfiguration
            categories={categories}
            existingRules={rewardRules}
            onSave={handleSaveRewardRules}
            saving={rewardLoading}
            error={rewardError}
          />
        )}

        {activeTab === "settings" && (
          <ProfileSettings
            profile={enterpriseProfile}
            email={user?.email ?? ""}
            onLogout={logout}
          />
        )}

        {activeTab === "collectors" && (
          <CollectorsManagement
            collectors={collectors}
            loading={loading}
            error={null}
            onRefresh={refreshCollectors}
          />
        )}
      </section>
    </div>
  );
};