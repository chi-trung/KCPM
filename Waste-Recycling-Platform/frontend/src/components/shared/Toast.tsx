import React, { useState, useCallback } from 'react';
import { X, CheckCircle, AlertCircle, Info } from 'lucide-react';
import { Portal } from './Portal';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info';
  message: string;
}

export const useToast = () => {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const addToast = useCallback((type: 'success' | 'error' | 'info', message: string) => {
    const id = Math.random().toString(36).substring(2, 9);
    setToasts((prev) => [...prev, { id, type, message }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 3000);
  }, []);

  const success = useCallback((msg: string) => addToast('success', msg), [addToast]);
  const error = useCallback((msg: string) => addToast('error', msg), [addToast]);
  const info = useCallback((msg: string) => addToast('info', msg), [addToast]);
  
  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return { toasts, success, error, info, removeToast };
};

export const ToastContainer = ({ toasts, onRemove }: { toasts: ToastMessage[], onRemove: (id: string) => void }) => {
  return (
    <Portal>
      <div className="fixed top-4 right-4 z-[9999] flex flex-col gap-2">
        {toasts.map((t) => (
          <div key={t.id} className={`flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg min-w-[250px] animate-in slide-in-from-right-8 fade-in ${
            t.type === 'success' ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' :
            t.type === 'error' ? 'bg-red-50 text-red-800 border border-red-200' :
            'bg-blue-50 text-blue-800 border border-blue-200'
          }`}>
            {t.type === 'success' && <CheckCircle size={20} className="text-emerald-600" />}
            {t.type === 'error' && <AlertCircle size={20} className="text-red-600" />}
            {t.type === 'info' && <Info size={20} className="text-blue-600" />}
            <p className="text-sm font-medium flex-1">{t.message}</p>
            <button onClick={() => onRemove(t.id)} className="text-gray-400 hover:text-gray-600">
              <X size={16} />
            </button>
          </div>
        ))}
      </div>
    </Portal>
  );
};