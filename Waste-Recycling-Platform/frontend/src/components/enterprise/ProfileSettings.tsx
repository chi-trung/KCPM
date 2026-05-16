import React, { useState } from "react";
import { Button, Card, Input, Modal } from "../ui";
import { EnterpriseProfile } from "../../lib/api/enterpriseTaskApi";

interface ProfileSettingsProps {
  profile: EnterpriseProfile;
  email: string;
  onLogout: () => void;
}

export const ProfileSettings: React.FC<ProfileSettingsProps> = ({ profile, email, onLogout }) => {
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [passwordMessage, setPasswordMessage] = useState<string | null>(null);
  const [isLogoutModalOpen, setIsLogoutModalOpen] = useState(false);

  const handlePasswordSubmit = () => {
    if (!currentPassword || !newPassword || !confirmPassword) {
      setPasswordMessage("Please fill all password fields.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setPasswordMessage("New password and confirmation do not match.");
      return;
    }

    setPasswordMessage("Change password endpoint is not available yet.");
  };

  const handleConfirmLogout = () => {
    setIsLogoutModalOpen(false);
    onLogout();
  };

  return (
    <div className="space-y-6">
      <Card className="p-6">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h3 className="text-xl font-bold text-gray-900">Enterprise Profile</h3>
            <p className="mt-1 text-sm text-gray-600">View your enterprise account information.</p>
          </div>
          <Button variant="danger" size="sm" onClick={() => setIsLogoutModalOpen(true)}>
            Đăng xuất
          </Button>
        </div>

        <div className="mt-5 grid grid-cols-1 gap-4 md:grid-cols-2">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Company Name</label>
            <Input value={profile.companyName} disabled />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Email</label>
            <Input value={email} disabled />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Service Area</label>
            <Input value={profile.serviceArea ?? ""} disabled />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Capacity (kg/day)</label>
            <Input value={String(profile.capacityKgPerDay ?? "")} disabled />
          </div>
        </div>
      </Card>

      <Card className="p-6">
        <h3 className="text-xl font-bold text-gray-900">Change Password</h3>
        <p className="mt-1 text-sm text-gray-600">Update password for enterprise account security.</p>

        {passwordMessage && (
          <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
            {passwordMessage}
          </div>
        )}

        <div className="mt-5 grid grid-cols-1 gap-4 md:grid-cols-3">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Current Password</label>
            <Input
              type="password"
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">New Password</label>
            <Input
              type="password"
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">Confirm Password</label>
            <Input
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
            />
          </div>
        </div>

        <div className="mt-5">
          <Button onClick={handlePasswordSubmit}>Change Password</Button>
        </div>
      </Card>

      <Modal
        isOpen={isLogoutModalOpen}
        title="Xác nhận đăng xuất"
        onClose={() => setIsLogoutModalOpen(false)}
        onConfirm={handleConfirmLogout}
        confirmText="Đăng xuất"
        cancelText="Ở lại"
        size="sm"
      >
        <p className="text-sm text-gray-700">
          Bạn có chắc muốn đăng xuất khỏi tài khoản doanh nghiệp này không?
        </p>
      </Modal>
    </div>
  );
};
