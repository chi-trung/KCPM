import React, { useRef, useEffect, useState } from "react";

interface DropdownProps {
  trigger: React.ReactNode;
  items: DropdownItem[];
  align?: "left" | "right";
  className?: string;
}

interface DropdownItem {
  label: string;
  onClick: () => void;
  icon?: React.ReactNode;
  divider?: boolean;
  danger?: boolean;
}

export const Dropdown: React.FC<DropdownProps> = ({
  trigger,
  items,
  align = "right",
  className,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleItemClick = (callback: () => void) => {
    callback();
    setIsOpen(false);
  };

  return (
    <div
      ref={containerRef}
      className={`relative inline-block ${className || ""}`}
    >
      {/* Trigger */}
      <div onClick={() => setIsOpen(!isOpen)} className="cursor-pointer">
        {trigger}
      </div>

      {/* Menu */}
      {isOpen && (
        <div
          className={`
            absolute top-full mt-2 bg-white rounded-lg shadow-lg border border-gray-200
            min-w-max z-50
            ${align === "right" ? "right-0" : "left-0"}
          `}
        >
          {items.map((item, index) =>
            item.divider ? (
              <div key={index} className="border-t border-gray-200" />
            ) : (
              <button
                key={index}
                onClick={() => handleItemClick(item.onClick)}
                className={`
                  w-full text-left px-4 py-2 flex items-center gap-2
                  hover:bg-gray-100 transition-colors
                  ${item.danger ? "text-red-600 hover:bg-red-50" : "text-gray-700"}
                `}
              >
                {item.icon && <span className="w-4 h-4">{item.icon}</span>}
                {item.label}
              </button>
            ),
          )}
        </div>
      )}
    </div>
  );
};
