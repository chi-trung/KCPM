import React, { useEffect, useMemo, useState } from "react";
import { Badge, Button, Card, Input, Modal } from "../ui";
import { EnterpriseCollector } from "../../lib/api/enterpriseTaskApi";
import { enterpriseCollectorApi } from "../../lib/api/enterpriseCollectorApi";
import { Users, UserCheck, UserX } from "lucide-react";

interface CollectorsManagementProps {
  collectors: EnterpriseCollector[];
  loading: boolean;
  error: string | null;
  onRefresh: () => Promise<void>;
}

export const CollectorsManagement: React.FC<CollectorsManagementProps> = ({
  collectors,
  loading,
  error,
  onRefresh,
}) => {
  const [actionError, setActionError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);

  const [selectedCollector, setSelectedCollector] = useState<EnterpriseCollector | null>(null);

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [temporaryPassword, setTemporaryPassword] = useState("");
  const [isAvailable, setIsAvailable] = useState(true);

  const availableCount = collectors.filter((collector) => collector.isAvailable).length;

  // Auto load collectors when component mounts
  useEffect(() => {
    onRefresh();
  }, []);

  const clearForm = () => {
    setFullName("");
    setEmail("");
    setPhone("");
    setTemporaryPassword("");
    setIsAvailable(true);
  };

  const openCreateModal = () => {
    clearForm();
    setActionError(null);
    setCreateModalOpen(true);
  };

  const openEditModal = (collector: EnterpriseCollector) => {
    setSelectedCollector(collector);
    setFullName(collector.name);
    setEmail(collector.email);
    setPhone(collector.phone ?? "");
    setTemporaryPassword("");
    setIsAvailable(collector.isAvailable);
    setActionError(null);
    setEditModalOpen(true);
  };

  const openDeleteModal = (collector: EnterpriseCollector) => {
    setSelectedCollector(collector);
    setActionError(null);
    setDeleteModalOpen(true);
  };

  const canSubmit = useMemo(() => {
    return fullName.trim().length > 0 && email.trim().length > 0;
  }, [fullName, email]);

  const handleCreateCollector = async () => {
    if (!canSubmit || temporaryPassword.trim().length < 6) {
      setActionError("Please provide full name, email, and a temporary password with at least 6 characters.");
      return;
    }

    setSubmitting(true);
    setActionError(null);
    try {
      await enterpriseCollectorApi.createCollector({
        fullName: fullName.trim(),
        email: email.trim(),
        phone: phone.trim() || undefined,
        temporaryPassword: temporaryPassword.trim(),
        isAvailable,
      });
      setCreateModalOpen(false);
      clearForm();
      await onRefresh();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Failed to create collector.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdateCollector = async () => {
    if (!selectedCollector) return;
    if (!canSubmit) {
      setActionError("Full name and email are required.");
      return;
    }

    if (temporaryPassword.trim().length > 0 && temporaryPassword.trim().length < 6) {
      setActionError("Temporary password must be at least 6 characters.");
      return;
    }

    setSubmitting(true);
    setActionError(null);
    try {
      await enterpriseCollectorApi.updateCollector(selectedCollector.id, {
        fullName: fullName.trim(),
        email: email.trim(),
        phone: phone.trim() || undefined,
        temporaryPassword: temporaryPassword.trim() || undefined,
        isAvailable,
      });
      setEditModalOpen(false);
      setSelectedCollector(null);
      clearForm();
      await onRefresh();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Failed to update collector.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteCollector = async () => {
    if (!selectedCollector) return;

    setSubmitting(true);
    setActionError(null);
    try {
      await enterpriseCollectorApi.deleteCollector(selectedCollector.id);
      setDeleteModalOpen(false);
      setSelectedCollector(null);
      await onRefresh();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Failed to delete collector.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <Card className="p-6">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h3 className="text-xl font-bold text-gray-900">Collectors</h3>
            <p className="mt-1 text-sm text-gray-600">
              Track collector status and task workload in real time.
            </p>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={openCreateModal}>
              Add Collector
            </Button>
            <Button onClick={onRefresh} isLoading={loading}>
              Refresh
            </Button>
          </div>
        </div>
      </Card>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <Card className="p-5">
          <div className="flex items-center gap-3">
            <Users className="h-5 w-5 text-sky-600" />
            <div>
              <p className="text-sm text-gray-500">Total Collectors</p>
              <p className="text-2xl font-bold text-gray-900">{collectors.length}</p>
            </div>
          </div>
        </Card>

        <Card className="p-5">
          <div className="flex items-center gap-3">
            <UserCheck className="h-5 w-5 text-emerald-600" />
            <div>
              <p className="text-sm text-gray-500">Available</p>
              <p className="text-2xl font-bold text-emerald-700">{availableCount}</p>
            </div>
          </div>
        </Card>

        <Card className="p-5">
          <div className="flex items-center gap-3">
            <UserX className="h-5 w-5 text-rose-600" />
            <div>
              <p className="text-sm text-gray-500">Busy / Offline</p>
              <p className="text-2xl font-bold text-rose-700">{collectors.length - availableCount}</p>
            </div>
          </div>
        </Card>
      </div>

      <Card className="overflow-hidden">
        <div className="border-b border-gray-100 px-6 py-4">
          <h4 className="font-semibold text-gray-900">Collector Directory</h4>
        </div>

        {error && (
          <div className="mx-6 mt-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
            {error}
          </div>
        )}

        {collectors.length === 0 ? (
          <div className="px-6 py-10 text-center text-sm text-gray-500">
            No collectors found for this enterprise.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50 text-left text-xs uppercase tracking-wide text-gray-500">
                <tr>
                  <th className="px-6 py-3">Collector</th>
                  <th className="px-6 py-3">Contact</th>
                  <th className="px-6 py-3">Status</th>
                  <th className="px-6 py-3">Active Tasks</th>
                  <th className="px-6 py-3">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 bg-white text-sm">
                {collectors.map((collector) => (
                  <tr key={collector.id}>
                    <td className="px-6 py-4">
                      <p className="font-medium text-gray-900">{collector.name}</p>
                      <p className="text-xs text-gray-500">Joined {new Date(collector.createdAt).toLocaleDateString("vi-VN")}</p>
                    </td>
                    <td className="px-6 py-4">
                      <p className="text-gray-700">{collector.email}</p>
                      <p className="text-xs text-gray-500">{collector.phone || "No phone"}</p>
                    </td>
                    <td className="px-6 py-4">
                      <Badge variant={collector.isAvailable ? "success" : "warning"} size="sm">
                        {collector.isAvailable ? "Available" : "Busy"}
                      </Badge>
                    </td>
                    <td className="px-6 py-4 text-gray-700">{collector.taskCount}</td>
                    <td className="px-6 py-4">
                      <div className="flex gap-2">
                        <Button size="sm" variant="outline" onClick={() => openEditModal(collector)}>
                          Edit
                        </Button>
                        <Button size="sm" variant="danger" onClick={() => openDeleteModal(collector)}>
                          Delete
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal
        isOpen={createModalOpen}
        title="Add Collector"
        onClose={() => {
          setCreateModalOpen(false);
          clearForm();
          setActionError(null);
        }}
        onConfirm={handleCreateCollector}
        confirmText={submitting ? "Creating..." : "Create Collector"}
      >
        <div className="space-y-4">
          {actionError && (
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{actionError}</div>
          )}

          <Input label="Full Name" value={fullName} onChange={(event) => setFullName(event.target.value)} />
          <Input label="Email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
          <Input label="Phone" value={phone} onChange={(event) => setPhone(event.target.value)} />
          <Input
            label="Temporary Password"
            type="password"
            value={temporaryPassword}
            onChange={(event) => setTemporaryPassword(event.target.value)}
            helperText="Collector will use this password to sign in the first time."
          />

          <label className="flex items-center gap-2 text-sm text-gray-700">
            <input
              type="checkbox"
              checked={isAvailable}
              onChange={(event) => setIsAvailable(event.target.checked)}
              className="h-4 w-4 rounded border-gray-300"
            />
            Set collector as available
          </label>
        </div>
      </Modal>

      <Modal
        isOpen={editModalOpen}
        title="Edit Collector"
        onClose={() => {
          setEditModalOpen(false);
          setSelectedCollector(null);
          clearForm();
          setActionError(null);
        }}
        onConfirm={handleUpdateCollector}
        confirmText={submitting ? "Saving..." : "Save Changes"}
      >
        <div className="space-y-4">
          {actionError && (
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{actionError}</div>
          )}

          <Input label="Full Name" value={fullName} onChange={(event) => setFullName(event.target.value)} />
          <Input label="Email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
          <Input label="Phone" value={phone} onChange={(event) => setPhone(event.target.value)} />
          <Input
            label="Reset Temporary Password (optional)"
            type="password"
            value={temporaryPassword}
            onChange={(event) => setTemporaryPassword(event.target.value)}
            helperText="Leave blank to keep current password."
          />

          <label className="flex items-center gap-2 text-sm text-gray-700">
            <input
              type="checkbox"
              checked={isAvailable}
              onChange={(event) => setIsAvailable(event.target.checked)}
              className="h-4 w-4 rounded border-gray-300"
            />
            Collector is available
          </label>
        </div>
      </Modal>

      <Modal
        isOpen={deleteModalOpen}
        title="Delete Collector"
        onClose={() => {
          setDeleteModalOpen(false);
          setSelectedCollector(null);
          setActionError(null);
        }}
        onConfirm={handleDeleteCollector}
        confirmText={submitting ? "Deleting..." : "Delete"}
      >
        <div className="space-y-4">
          {actionError && (
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{actionError}</div>
          )}

          <p className="text-sm text-gray-700">
            Are you sure you want to delete collector <span className="font-semibold">{selectedCollector?.name}</span>?
          </p>
          <p className="text-xs text-gray-500">
            Deletion is blocked if the collector still has active tasks.
          </p>
        </div>
      </Modal>
    </div>
  );
};
