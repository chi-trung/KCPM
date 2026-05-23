import React from "react";

interface AvatarProps {
  src?: string;
  alt?: string;
  initials?: string;
  size?: "sm" | "md" | "lg" | "xl";
  className?: string;
}

const sizeStyles = {
  sm: "w-8 h-8 text-xs",
  md: "w-10 h-10 text-sm",
  lg: "w-12 h-12 text-base",
  xl: "w-16 h-16 text-lg",
};

export const Avatar: React.FC<AvatarProps> = ({
  src,
  alt = "User avatar",
  initials = "?",
  size = "md",
  className,
}) => {
  return (
    <div
      className={`
        ${sizeStyles[size]}
        flex items-center justify-center rounded-full font-semibold
        ${src ? "overflow-hidden" : "bg-amber-100 text-amber-700"}
        ${className || ""}
      `}
    >
      {src ? (
        <img src={src} alt={alt} className="w-full h-full object-cover" />
      ) : (
        initials
      )}
    </div>
  );
};
