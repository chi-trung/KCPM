"use client";

import React, { useEffect } from "react";

interface ToastProps {
  id: string;
  message: string;
  type?: "success" | "error" | "warning" | "info";
  duration?: number;
  onClose: (id: string) => void;
}

interface ToastContextType {
  toasts: ToastProps[];
  addToast: (
    message: string,
    type?: "success" | "error" | "warning" | "info",
  ) => void;
  removeToast: (id: string) => void;
}

export const ToastContext = React.createContext<ToastContextType | undefined>(
  undefined,
);

export const useToast = () => {
  const context = React.useContext(ToastContext);
  if (!context) {
    throw new Error("useToast must be used within ToastProvider");
  }
  return context;
};

interface ToastItemProps {
  toast: ToastProps;
  onClose: (id: string) => void;
}

const toastTypeStyles = {
  success: {
    bg: "bg-green-50",
    border: "border-green-200",
    icon: "✓",
    iconColor: "text-green-600",
    textColor: "text-green-800",
  },
  error: {
    bg: "bg-red-50",
    border: "border-red-200",
    icon: "✕",
    iconColor: "text-red-600",
    textColor: "text-red-800",
  },
  warning: {
    bg: "bg-yellow-50",
    border: "border-yellow-200",
    icon: "!",
    iconColor: "text-yellow-600",
    textColor: "text-yellow-800",
  },
  info: {
    bg: "bg-blue-50",
    border: "border-blue-200",
    icon: "ℹ",
    iconColor: "text-blue-600",
    textColor: "text-blue-800",
  },
};

const ToastItem: React.FC<ToastItemProps> = ({ toast, onClose }) => {
  const styles = toastTypeStyles[toast.type || "info"];

  useEffect(() => {
    if (toast.duration && toast.duration > 0) {
      const timer = setTimeout(() => onClose(toast.id), toast.duration);
      return () => clearTimeout(timer);
    }
  }, [toast.id, toast.duration, onClose]);

  return (
    <div
      className={`
        ${styles.bg} ${styles.border} border rounded-lg p-4 mb-3
        flex items-start gap-3 shadow-lg animation-slide-in
      `}
    >
      <span className={`text-lg font-bold flex-shrink-0 ${styles.iconColor}`}>
        {styles.icon}
      </span>
      <p className={`flex-1 text-sm font-medium ${styles.textColor}`}>
        {toast.message}
      </p>
      <button
        onClick={() => onClose(toast.id)}
        className={`text-lg font-bold hover:opacity-70 flex-shrink-0 ${styles.iconColor}`}
      >
        ×
      </button>
    </div>
  );
};

interface ToastContainerProps {
  toasts: ToastProps[];
  onClose: (id: string) => void;
}

export const ToastContainer: React.FC<ToastContainerProps> = ({
  toasts,
  onClose,
}) => {
  return (
    <div className="fixed bottom-4 right-4 z-50 max-w-sm">
      {toasts.map((toast) => (
        <ToastItem key={toast.id} toast={toast} onClose={onClose} />
      ))}
    </div>
  );
};

interface ToastProviderProps {
  children: React.ReactNode;
}

export const ToastProvider: React.FC<ToastProviderProps> = ({ children }) => {
  const [toasts, setToasts] = React.useState<ToastProps[]>([]);

  const addToast = React.useCallback(
    (
      message: string,
      type: "success" | "error" | "warning" | "info" = "info",
    ) => {
      const id = Date.now().toString();
      const duration = type === "error" ? 6000 : 4000;

      setToasts((prev) => [
        ...prev,
        { id, message, type, duration, onClose: removeToast },
      ]);
    },
    [],
  );

  const removeToast = React.useCallback((id: string) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ toasts, addToast, removeToast }}>
      {children}
      <ToastContainer toasts={toasts} onClose={removeToast} />
    </ToastContext.Provider>
  );
};
