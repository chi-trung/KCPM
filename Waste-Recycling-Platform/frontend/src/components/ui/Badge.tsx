import React from "react";

interface BadgeProps {
  variant?: "primary" | "success" | "warning" | "danger" | "info" | "default";
  size?: "sm" | "md";
  children: React.ReactNode;
  className?: string;
}

const variantStyles = {
  primary: "bg-blue-100 text-blue-800",
  success: "bg-green-100 text-green-800",
  warning: "bg-yellow-100 text-yellow-800",
  danger: "bg-red-100 text-red-800",
  info: "bg-cyan-100 text-cyan-800",
  default: "bg-gray-100 text-gray-800",
};

const sizeStyles = {
  sm: "px-2 py-0.5 text-xs font-semibold",
  md: "px-3 py-1 text-sm font-semibold",
};

export const Badge: React.FC<BadgeProps> = ({
  variant = "default",
  size = "md",
  children,
  className,
}) => {
  return (
    <span
      className={`
      inline-block rounded-full
      ${variantStyles[variant]}
      ${sizeStyles[size]}
      ${className || ""}
    `}
    >
      {children}
    </span>
  );
};
