import React, { useState } from "react";
import Link from "next/link";

interface SidebarProps {
  items: SidebarItem[];
  isOpen?: boolean;
  onClose?: () => void;
  footer?: React.ReactNode;
}

interface SidebarItem {
  label: string;
  href: string;
  icon?: React.ReactNode;
  badge?: number | string;
  children?: SidebarItem[];
}

interface SidebarItemComponentProps {
  item: SidebarItem;
  isActive?: boolean;
  level?: number;
}

const SidebarItemComponent: React.FC<SidebarItemComponentProps> = ({
  item,
  isActive,
  level = 0,
}) => {
  const [isExpanded, setIsExpanded] = useState(false);
  const hasChildren = item.children && item.children.length > 0;
  const paddingLeft = `${level * 1.5}rem`;

  return (
    <div>
      <div style={{ paddingLeft }} className="relative">
        <Link
          href={item.href}
          className={`
            flex items-center gap-3 px-4 py-3 rounded-md transition-colors
            ${
              isActive
                ? "bg-amber-100 text-amber-700 font-semibold"
                : "text-gray-700 hover:bg-gray-100"
            }
          `}
          onClick={() => hasChildren && setIsExpanded(!isExpanded)}
        >
          {item.icon && <span className="w-5 h-5">{item.icon}</span>}
          <span className="flex-1">{item.label}</span>
          {item.badge && (
            <span className="bg-amber-600 text-white text-xs font-bold rounded-full px-2 py-1">
              {item.badge}
            </span>
          )}
          {hasChildren && (
            <svg
              className={`w-4 h-4 transition-transform ${
                isExpanded ? "rotate-180" : ""
              }`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 14l-7 7m0 0l-7-7m7 7V3"
              />
            </svg>
          )}
        </Link>
      </div>

      {/* Submenu */}
      {hasChildren && isExpanded && (
        <div className="mt-1">
          {item.children?.map((child) => (
            <SidebarItemComponent
              key={child.href}
              item={child}
              level={level + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export const Sidebar: React.FC<SidebarProps> = ({
  items,
  isOpen = true,
  onClose,
  footer,
}) => {
  return (
    <>
      {/* Mobile backdrop */}
      {!isOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 md:hidden z-30"
          onClick={onClose}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`
          fixed md:static inset-y-0 left-0 z-40
          w-64 bg-white border-r border-gray-200
          transform transition-transform duration-300 md:translate-x-0 flex flex-col
          ${isOpen ? "translate-x-0" : "-translate-x-full"}
        `}
      >
        {/* Sidebar Content */}
        <div className="flex-1 overflow-y-auto py-6">
          <nav className="px-3 space-y-2">
            {items.map((item) => (
              <SidebarItemComponent key={item.href} item={item} />
            ))}
          </nav>
        </div>

        {/* Sidebar Footer */}
        {footer && <div className="border-t border-gray-200 p-4">{footer}</div>}
      </aside>
    </>
  );
};
