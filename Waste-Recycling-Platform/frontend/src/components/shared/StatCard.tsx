import React from "react";
import { Card } from "@/components/ui";

export interface StatCardProps {
  icon?: React.ReactNode;
  label: string;
  value: string | number;
  unit?: string;
  trend?: "up" | "down" | "neutral";
  trendValue?: string;
  color?: "amber" | "green" | "blue" | "red";
  className?: string;
}

const colorStyles = {
  amber: "bg-amber-50 text-amber-700 border-amber-200",
  green: "bg-green-50 text-green-700 border-green-200",
  blue: "bg-blue-50 text-blue-700 border-blue-200",
  red: "bg-red-50 text-red-700 border-red-200",
};

export const StatCard: React.FC<StatCardProps> = ({
  icon,
  label,
  value,
  unit,
  trend,
  trendValue,
  color = "amber",
  className,
}) => {
  return (
    <Card className={`border-2 ${colorStyles[color]} ${className || ""}`}>
      <Card.Body className="space-y-3">
        {/* Icon & Label */}
        <div className="flex items-center justify-between">
          {icon && <span className="text-3xl">{icon}</span>}
          <p className="text-sm font-medium">{label}</p>
        </div>

        {/* Value */}
        <div className="flex items-baseline gap-2">
          <p className="text-3xl font-bold">{value}</p>
          {unit && <p className="text-sm text-gray-600">{unit}</p>}
        </div>

        {/* Trend */}
        {trend && trendValue && (
          <p
            className={`text-sm font-medium ${
              trend === "up"
                ? "text-green-600"
                : trend === "down"
                  ? "text-red-600"
                  : "text-gray-600"
            }`}
          >
            {trend === "up" ? "↑" : trend === "down" ? "↓" : "→"} {trendValue}
          </p>
        )}
      </Card.Body>
    </Card>
  );
};
