import React from "react";

interface ProgressProps {
  value: number;
  max?: number;
  showLabel?: boolean;
  size?: "sm" | "md" | "lg";
  color?: "amber" | "green" | "blue" | "red";
  className?: string;
}

const colorStyles = {
  amber: "bg-amber-600",
  green: "bg-green-600",
  blue: "bg-blue-600",
  red: "bg-red-600",
};

const sizeStyles = {
  sm: "h-2",
  md: "h-3",
  lg: "h-4",
};

export const Progress: React.FC<ProgressProps> = ({
  value,
  max = 100,
  showLabel = false,
  size = "md",
  color = "amber",
  className,
}) => {
  const percentage = (value / max) * 100;

  return (
    <div className={className}>
      <div
        className={`w-full bg-gray-200 rounded-full overflow-hidden ${sizeStyles[size]}`}
      >
        <div
          className={`${colorStyles[color]} h-full transition-all duration-300`}
          style={{ width: `${percentage}%` }}
        />
      </div>
      {showLabel && (
        <p className="text-sm text-gray-600 mt-2 text-center">
          {Math.round(percentage)}%
        </p>
      )}
    </div>
  );
};
