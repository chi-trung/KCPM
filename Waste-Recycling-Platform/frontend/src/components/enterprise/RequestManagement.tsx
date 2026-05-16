import React, { useState } from "react";
import { 
  Card, 
  Button, 
  Badge, 
  Table, 
  Select, 
  Modal 
} from "../ui";
import { reportApi } from "../../lib/api/reportApi";
import { EnterpriseRequest } from "./types";
import { ReportDetailModal } from "../citizen/ReportDetailModal";

interface RequestManagementProps {
  requests: EnterpriseRequest[];
  onStatusChange: (reportId: string, status: string) => void;
  onAssign: (reportId: string, collectorId: string) => void;
}

const MOCK_COLLECTORS = [
  { value: "c1", label: "Collector 1 (Availability: High)" },
  { value: "c2", label: "Collector 2 (Availability: Low)" }, 
  { value: "c3", label: "Collector 3 (Availability: Medium)" },
];

export const RequestManagement: React.FC<RequestManagementProps> = ({ requests, onStatusChange, onAssign }) => {
  const [selectedRequest, setSelectedRequest] = useState<EnterpriseRequest | null>(null);
  const [assignModalOpen, setAssignModalOpen] = useState(false);
  const [selectedCollector, setSelectedCollector] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [detailReportId, setDetailReportId] = useState<string | null>(null);

  const handleAccept = async (reportId: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await reportApi.acceptReport(reportId);
      onStatusChange(reportId, "Accepted");
      alert(response.message);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Failed to accept report";
      setError(errorMessage);
      alert(`Error: ${errorMessage}`);
    } finally {
      setLoading(false);
    }
  };

  const handleReject = async (reportId: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await reportApi.rejectReport(reportId);
      onStatusChange(reportId, "Rejected");
      alert(response.message);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Failed to reject report";
      setError(errorMessage);
      alert(`Error: ${errorMessage}`);
    } finally {
      setLoading(false);
    }
  };

  const handleAssignClick = () => {
    if (selectedRequest && selectedCollector) {
      onAssign(selectedRequest.reportId, selectedCollector);
      setAssignModalOpen(false);
      setSelectedCollector("");
      setSelectedRequest(null);
    }
  };

  return (
    <Card className="p-0 overflow-hidden">
       <div className="p-6 border-b border-gray-100 flex justify-between items-center">
         <h2 className="text-xl font-bold text-gray-800">Incoming Collection Requests</h2>
         <div className="flex gap-2">
           <Button variant="outline" size="sm">Filter</Button>
           <Button variant="outline" size="sm">Export</Button>
         </div>
       </div>
       <Table
          data={requests}
          onRowClick={(row) => setDetailReportId(row.reportId)}
          columns={[
            { label: "ID", key: "reportId" },
            { label: "Type", key: "type" },
            { label: "Quantity", key: "quantity" },
            { label: "Location", key: "location" },
            { label: "Date", key: "date" },
            { 
               label: "Status", 
               key: "status",
               render: (val: string) => (
                 <Badge variant={val === "Accepted" ? "success" : val === "Pending" ? "warning" : "default"}>
                   {val}
                 </Badge>
               )
            },
            {
               label: "Actions",
               key: "id",
               render: (_: any, row: any) => (
                 <div className="flex gap-2">
                   {row.status === "Pending" && (
                     <>
                       <Button 
                         size="sm" 
                         variant="primary" 
                         onClick={() => handleAccept(row.reportId)}
                         disabled={loading}
                       >
                         {loading ? "Processing..." : "Approve"}
                       </Button>
                       <Button 
                         size="sm" 
                         variant="danger" 
                         onClick={() => handleReject(row.reportId)}
                         disabled={loading}
                       >
                         {loading ? "Processing..." : "Reject"}
                       </Button>
                     </>
                   )}
                   {row.status === "Accepted" && (
                      <Button 
                        size="sm" 
                        variant="secondary"
                        onClick={() => {
                          setSelectedRequest(row);
                          setAssignModalOpen(true);
                        }}
                      >
                        Assign Collector
                      </Button>
                   )}
                 </div>
               )
            }
          ]}
       />

      {/* Assign Collector Modal */}
      <Modal 
        isOpen={assignModalOpen} 
        onClose={() => setAssignModalOpen(false)}
        title="Assign Collector"
        onConfirm={handleAssignClick}
        confirmText="Confirm Assignment"
      >
        <div className="space-y-4">
           <p className="text-sm text-gray-600">
             Assigning request <b>#{selectedRequest?.reportId}</b> ({selectedRequest?.type}, {selectedRequest?.quantity})
           </p>
           
           <div>
             <label className="block text-sm font-medium text-gray-700 mb-1">Select Collector</label>
             <Select 
               options={MOCK_COLLECTORS}
               value={selectedCollector}
               onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setSelectedCollector(e.target.value)}
               placeholder="Choose a Collector..."
             />
           </div>
        </div>
      </Modal>

      {/* Report Detail Modal */}
      {detailReportId && (
        <ReportDetailModal
          reportId={detailReportId}
          onClose={() => setDetailReportId(null)}
        />
      )}
    </Card>
  );
};