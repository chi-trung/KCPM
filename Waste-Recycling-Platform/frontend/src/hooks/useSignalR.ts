import { useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { API_CONFIG } from '@/lib/api/config';

export interface NotificationPayload {
  id: string;
  type: string;
  title: string;
  message: string;
  actionUrl?: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  createdAt: string;
}

interface UseSignalRProps {
  enabled: boolean;
  token: string | null;
  onNewNotification?: (notification: NotificationPayload) => void;
  onTaskStatusUpdated?: (taskId: string, status: string) => void;
  onComplaintResolved?: (complaintId: string, message: string, adminResponse: string) => void;
  onError?: (error: Error) => void;
}

export const useSignalR = ({ 
  enabled, 
  token,
  onNewNotification,
  onTaskStatusUpdated, 
  onComplaintResolved, 
  onError 
}: UseSignalRProps) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  
  // Use refs to store callbacks to avoid stale closure issues
  const callbacksRef = useRef({
    onNewNotification,
    onTaskStatusUpdated,
    onComplaintResolved,
    onError
  });
  
  // Update refs when callbacks change
  useEffect(() => {
    callbacksRef.current = {
      onNewNotification,
      onTaskStatusUpdated,
      onComplaintResolved,
      onError
    };
  }, [onNewNotification, onTaskStatusUpdated, onComplaintResolved, onError]);

  useEffect(() => {
    if (!enabled || !token) return;

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_CONFIG.SERVER_URL}/hubs/task`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    setConnection(newConnection);

    return () => {
      newConnection.stop();
    };
  }, [enabled, token]);

  useEffect(() => {
    if (!connection) return;

    if (connection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }

    connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      setIsConnected(false);
    });

    connection.onreconnected(() => {
      console.log('SignalR reconnected');
      setIsConnected(true);
    });

    connection.onclose(() => {
      console.log('SignalR connection closed');
      setIsConnected(false);
    });

    // Use refs in event handlers to get latest callbacks
    connection.on('NewNotification', (notification: NotificationPayload) => {
      console.log('New notification received:', notification);
      callbacksRef.current.onNewNotification?.(notification);
    });

    connection.on('TaskStatusUpdated', (taskId: string, status: string) => {
      callbacksRef.current.onTaskStatusUpdated?.(taskId, status);
    });

    connection.on('ComplaintResolved', (payload: any) => {
      const complaintId = payload?.complaintId ?? payload?.id ?? '';
      const message = payload?.message ?? '';
      const adminResponse = payload?.adminResponse ?? '';
      try {
        callbacksRef.current.onComplaintResolved?.(complaintId, message, adminResponse);
      } catch (e) {
        console.error('onComplaintResolved handler error', e);
      }
    });

    connection.start()
      .then(() => {
        console.log('SignalR Connected');
        setIsConnected(true);
      })
      .catch((e) => {
        console.error('SignalR Connection failed:', e);
        setIsConnected(false);
        callbacksRef.current.onError?.(e);
      });

    return () => {
      connection.off('NewNotification');
      connection.off('TaskStatusUpdated');
      connection.off('ComplaintResolved');
    };
  }, [connection]);

  return { connection, isConnected };
};