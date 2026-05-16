import React, { useState, useCallback } from 'react';
import { Portal } from './Portal';
import { AlertTriangle, HelpCircle } from 'lucide-react';

export interface ModalConfig {
  title: string;
  message: string;
  type: 'confirm' | 'prompt';
  placeholder?: string;
  confirmText?: string;
  cancelText?: string;
}

export const useConfirmation = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [config, setConfig] = useState<ModalConfig | null>(null);
  const [resolver, setResolver] = useState<{ resolve: (value: any) => void } | null>(null);

  const confirm = useCallback((cfg: ModalConfig) => {
    setIsOpen(true);
    setConfig({ ...cfg, type: 'confirm' });
    return new Promise<boolean>((resolve) => setResolver({ resolve }));
  }, []);

  const prompt = useCallback((cfg: Omit<ModalConfig, 'type'>) => {
    setIsOpen(true);
    setConfig({ ...cfg, type: 'prompt' });
    return new Promise<string | null>((resolve) => setResolver({ resolve }));
  }, []);

  const handleConfirm = (val: any) => {
    setIsOpen(false);
    if (resolver) resolver.resolve(val);
  };

  const handleCancel = () => {
    setIsOpen(false);
    if (resolver) resolver.resolve(config?.type === 'prompt' ? null : false);
  };

  return { isOpen, config, confirm, prompt, onConfirm: handleConfirm, onCancel: handleCancel };
};

export const ConfirmationModal = ({ isOpen, config, onConfirm, onCancel, isLoading }: any) => {
  const [inputValue, setInputValue] = useState('');

  if (!isOpen || !config) return null;

  return (
    <Portal>
      <div className="fixed inset-0 z-[9999] bg-slate-900/50 backdrop-blur-sm flex items-center justify-center animate-in fade-in duration-200">
        <div className="bg-white rounded-2xl p-6 max-w-sm w-full mx-4 shadow-2xl animate-in zoom-in-95 duration-200">
          <div className="flex items-center gap-3 mb-4">
            <div className="bg-amber-100 p-2 rounded-full text-amber-600">
              {config.type === 'confirm' ? <AlertTriangle size={24} /> : <HelpCircle size={24} />}
            </div>
            <h3 className="text-lg font-bold text-gray-900">{config.title}</h3>
          </div>
          <p className="text-gray-600 text-sm mb-4">{config.message}</p>
          
          {config.type === 'prompt' && (
            <input
              type="text"
              className="w-full p-3 border border-gray-200 rounded-xl mb-6 focus:ring-2 focus:ring-emerald-500 outline-none text-sm"
              placeholder={config.placeholder}
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
            />
          )}

          <div className="flex gap-3 mt-6">
            <button onClick={onCancel} disabled={isLoading} className="flex-1 py-2.5 rounded-xl font-bold text-sm bg-gray-100 hover:bg-gray-200 text-gray-700 transition-colors">
              {config.cancelText || 'Hủy'}
            </button>
            <button onClick={() => onConfirm(config.type === 'prompt' ? inputValue : true)} disabled={isLoading || (config.type === 'prompt' && !inputValue.trim())} className="flex-1 py-2.5 rounded-xl font-bold text-sm bg-emerald-600 hover:bg-emerald-700 text-white shadow-md disabled:opacity-50 transition-colors">
              {isLoading ? 'Đang xử lý...' : (config.confirmText || 'Xác nhận')}
            </button>
          </div>
        </div>
      </div>
    </Portal>
  );
};