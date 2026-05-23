import React, { useState, useEffect } from "react";
import { Card, Button, Input } from "../ui";

interface CapacitySettingsProps {
  profile: {
    companyName: string;
    serviceArea: string | null;
    capacityKgPerDay: number | null;
    status?: "Pending" | "Verified" | "Rejected";
    rejectionReason?: string | null;
  };
  categories: Array<{ id: number; name: string }>;
  acceptedIds: number[];
  onSave: (payload: { serviceArea: string; capacityKgPerDay: number | null; wasteCategoryIds: number[] }) => Promise<void>;
  saving: boolean;
  error: string | null;
}

export const CapacitySettings: React.FC<CapacitySettingsProps> = ({ profile, categories, acceptedIds, onSave, saving, error }) => {
  const [serviceArea, setServiceArea] = useState(profile.serviceArea ?? "");
  const [capacityKgPerDay, setCapacityKgPerDay] = useState<number | null>(profile.capacityKgPerDay);
  const [selectedWasteTypes, setSelectedWasteTypes] = useState<number[]>(acceptedIds);

  useEffect(() => {
    setServiceArea(profile.serviceArea ?? "");
    setCapacityKgPerDay(profile.capacityKgPerDay);
  }, [profile]);

  useEffect(() => {
    setSelectedWasteTypes(acceptedIds);
  }, [acceptedIds]);

  const handleCheckboxChange = (id: number, checked: boolean) => {
    if (checked) {
      setSelectedWasteTypes((prev) => [...prev, id]);
    } else {
      setSelectedWasteTypes((prev) => prev.filter((item) => item !== id));
    }
  };

  const handleSave = async () => {
    await onSave({
      serviceArea,
      capacityKgPerDay,
      wasteCategoryIds: selectedWasteTypes,
    });
  };

  return (
    <Card className="p-6 max-w-3xl">
      <h2 className="text-xl font-bold mb-6 text-gray-800">
        {profile.status === "Rejected" ? "✏️ Cập Nhật Hồ Sơ Mới" : "Capacity & Service Settings"}
      </h2>

      {profile.status === "Rejected" && profile.rejectionReason && (
        <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-6 rounded">
          <p className="font-semibold text-yellow-900 mb-1">⚠️ Lý do từ chối</p>
          <p className="text-sm text-yellow-800">{profile.rejectionReason}</p>
        </div>
      )}

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      <div className="space-y-6">
        <div>
          <p className="text-sm text-gray-600 mb-2">
            Enterprise: <span className="font-semibold text-gray-900">{profile.companyName}</span>
          </p>
        </div>

        <div className="grid gap-6 lg:grid-cols-2">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Service Area</label>
            <Input
              value={serviceArea}
              onChange={(e) => setServiceArea(e.target.value)}
              placeholder="Enter service area or supported districts"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Max Processing Capacity (kg/day)</label>
            <Input
              type="number"
              value={capacityKgPerDay ?? ""}
              onChange={(e) => setCapacityKgPerDay(e.target.value ? parseInt(e.target.value, 10) : null)}
              placeholder="Enter capacity in kg/day"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-3">Accepted Waste Types</label>
          <div className="grid gap-3 sm:grid-cols-2">
            {categories.length === 0 ? (
              <p className="text-sm text-gray-500">Loading categories...</p>
            ) : (
              categories.map((type) => (
                <label
                  key={type.id}
                  className="flex items-center gap-3 border rounded-xl p-4 hover:border-emerald-300 transition"
                >
                  <input
                    type="checkbox"
                    checked={selectedWasteTypes.includes(type.id)}
                    onChange={(e) => handleCheckboxChange(type.id, e.target.checked)}
                    className="h-4 w-4 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500"
                  />
                  <span className="text-sm font-medium text-gray-700">{type.name}</span>
                </label>
              ))
            )}
          </div>
        </div>

        <div className="pt-4">
          <Button onClick={handleSave} disabled={saving} className="w-full sm:w-auto">
            {saving ? "Saving..." : "Save Configuration"}
          </Button>
        </div>
      </div>
    </Card>
  );
};
