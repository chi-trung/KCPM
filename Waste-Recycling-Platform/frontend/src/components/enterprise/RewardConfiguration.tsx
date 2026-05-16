import React, { useEffect, useMemo, useState } from "react";
import { Card, Button, Input } from "../ui";
import { EnterpriseRewardRule, UpdateEnterpriseRewardRuleItem } from "../../lib/api/enterpriseRewardApi";

interface RewardConfigurationProps {
  categories: Array<{ id: number; name: string }>;
  existingRules: EnterpriseRewardRule[];
  onSave: (rules: UpdateEnterpriseRewardRuleItem[]) => Promise<void>;
  saving: boolean;
  error: string | null;
}

interface EditableRewardRule {
  wasteCategoryId: number;
  categoryName: string;
  pointsPerReport: number;
  bonusQuality: number;
  isActive: boolean;
}

export const RewardConfiguration: React.FC<RewardConfigurationProps> = ({
  categories,
  existingRules,
  onSave,
  saving,
  error,
}) => {
  const baseRules = useMemo<EditableRewardRule[]>(() => {
    const existingByCategory = new Map(
      existingRules.map((rule) => [rule.wasteCategoryId, rule])
    );

    return categories.map((category) => {
      const existing = existingByCategory.get(category.id);
      return {
        wasteCategoryId: category.id,
        categoryName: category.name,
        pointsPerReport: existing?.pointsPerReport ?? 10,
        bonusQuality: existing?.bonusQuality ?? 0,
        isActive: existing?.isActive ?? true,
      };
    });
  }, [categories, existingRules]);

  const [rewardRules, setRewardRules] = useState<EditableRewardRule[]>([]);

  useEffect(() => {
    setRewardRules(baseRules);
  }, [baseRules]);

  const handleNumberFieldUpdate = (
    index: number,
    field: "pointsPerReport" | "bonusQuality",
    value: string
  ) => {
    const parsedValue = value === "" ? 0 : parseInt(value, 10);
    const safeValue = Number.isFinite(parsedValue) && parsedValue >= 0 ? parsedValue : 0;

    setRewardRules((previousRules) => {
      const nextRules = [...previousRules];
      nextRules[index] = {
        ...nextRules[index],
        [field]: safeValue,
      };
      return nextRules;
    });
  };

  const handleToggleActive = (index: number, checked: boolean) => {
    setRewardRules((previousRules) => {
      const nextRules = [...previousRules];
      nextRules[index] = {
        ...nextRules[index],
        isActive: checked,
      };
      return nextRules;
    });
  };

  const handleSubmit = async () => {
    const payload: UpdateEnterpriseRewardRuleItem[] = rewardRules.map((rule) => ({
      wasteCategoryId: rule.wasteCategoryId,
      pointsPerReport: rule.pointsPerReport,
      bonusQuality: rule.bonusQuality,
      isActive: rule.isActive,
    }));

    await onSave(payload);
  };

  return (
    <Card className="p-6">
       <h2 className="text-xl font-bold mb-6 text-gray-800">Reward Configuration</h2>
       <div className="bg-blue-50 p-4 rounded-lg mb-6 text-blue-800 text-sm">
         Configure reward points for each waste category. These rules are used when a collection task is completed.
       </div>

       {error && (
         <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
           <p className="text-sm text-red-700">{error}</p>
         </div>
       )}

       <div className="space-y-4">
         {rewardRules.length === 0 ? (
           <p className="text-sm text-gray-500">No waste categories available to configure.</p>
         ) : (
           rewardRules.map((rule, idx) => (
             <div
               key={rule.wasteCategoryId}
               className="grid grid-cols-1 gap-4 border-b border-gray-100 pb-4 md:grid-cols-4 md:items-end"
             >
               <div>
                 <span className="text-sm font-bold text-gray-700">{rule.categoryName}</span>
               </div>
               <div>
                 <Input
                   type="number"
                   min={0}
                   value={rule.pointsPerReport}
                   onChange={(e) => handleNumberFieldUpdate(idx, "pointsPerReport", e.target.value)}
                   placeholder="Points per report"
                 />
                 <p className="mt-1 text-xs text-gray-500">Points per report</p>
               </div>
               <div>
                 <Input
                   type="number"
                   min={0}
                   value={rule.bonusQuality}
                   onChange={(e) => handleNumberFieldUpdate(idx, "bonusQuality", e.target.value)}
                   placeholder="Bonus quality"
                 />
                 <p className="mt-1 text-xs text-gray-500">Bonus quality</p>
               </div>
               <label className="flex items-center gap-3 text-sm text-gray-700">
                 <input
                   type="checkbox"
                   checked={rule.isActive}
                   onChange={(e) => handleToggleActive(idx, e.target.checked)}
                   className="h-4 w-4 rounded border-gray-300"
                 />
                 Active
               </label>
             </div>
           ))
         )}
         
         <div className="pt-4">
            <Button onClick={handleSubmit} disabled={saving || rewardRules.length === 0}>
              {saving ? "Saving..." : "Update Rules"}
            </Button>
         </div>
       </div>
    </Card>
  );
};