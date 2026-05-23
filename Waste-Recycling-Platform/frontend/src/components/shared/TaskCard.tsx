import React from "react";
import { Card, Badge, Button } from "@/components/ui";
import Link from "next/link";

export interface TaskCardProps {
  id: string;
  title: string;
  description: string;
  location: string;
  assignedTo?: string;
  status: "pending" | "in_progress" | "completed" | "cancelled";
  priority?: "low" | "medium" | "high";
  dueDate: string;
  reward?: number;
  acceptedAt?: string;
  onActionClick?: () => void;
  actionButtonLabel?: string;
}

const statusColors = {
  pending: "warning",
  in_progress: "info",
  completed: "success",
  cancelled: "danger",
} as const;

const statusLabels = {
  pending: "Pending",
  in_progress: "In Progress",
  completed: "Completed",
  cancelled: "Cancelled",
};

const priorityColors = {
  low: "info",
  medium: "warning",
  high: "danger",
} as const;

export const TaskCard: React.FC<TaskCardProps> = ({
  id,
  title,
  description,
  location,
  assignedTo,
  status,
  priority,
  dueDate,
  reward,
  acceptedAt,
  onActionClick,
  actionButtonLabel = "View Task",
}) => {
  const isOverdue =
    status !== "completed" &&
    status !== "cancelled" &&
    new Date(dueDate) < new Date();

  return (
    <Link href={`/tasks/${id}`}>
      <Card
        hoverable
        className={`h-full cursor-pointer ${
          isOverdue ? "border-2 border-red-400" : ""
        }`}
      >
        <Card.Body className="space-y-3">
          {/* Header */}
          <div className="flex items-start justify-between gap-2">
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
              {assignedTo && (
                <p className="text-xs text-gray-500 mt-1">
                  Assigned to: {assignedTo}
                </p>
              )}
            </div>
            <div className="flex gap-2">
              <Badge variant={statusColors[status]}>
                {statusLabels[status]}
              </Badge>
              {priority && (
                <Badge variant={priorityColors[priority]}>
                  {priority.charAt(0).toUpperCase() + priority.slice(1)}
                </Badge>
              )}
            </div>
          </div>

          {/* Description */}
          <p className="text-sm text-gray-600 line-clamp-2">{description}</p>

          {/* Info Grid */}
          <div className="grid grid-cols-2 gap-3 py-3 border-t border-b border-gray-200">
            <div>
              <p className="text-xs text-gray-500 uppercase">Location</p>
              <p className="text-sm font-medium text-gray-900 truncate">
                {location}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500 uppercase">Due Date</p>
              <p
                className={`text-sm font-medium ${
                  isOverdue ? "text-red-600" : "text-gray-900"
                }`}
              >
                {new Date(dueDate).toLocaleDateString()}
                {isOverdue && " ⚠️"}
              </p>
            </div>
            {acceptedAt && (
              <div>
                <p className="text-xs text-gray-500 uppercase">Accepted</p>
                <p className="text-sm font-medium text-gray-900">
                  {new Date(acceptedAt).toLocaleDateString()}
                </p>
              </div>
            )}
            {reward && (
              <div>
                <p className="text-xs text-gray-500 uppercase">Reward</p>
                <p className="text-sm font-bold text-amber-600">🏆 {reward}</p>
              </div>
            )}
          </div>

          {/* Action Button */}
          <Button
            variant="primary"
            size="sm"
            className="w-full"
            onClick={(e: React.MouseEvent) => {
              e.preventDefault();
              onActionClick?.();
            }}
          >
            {actionButtonLabel}
          </Button>
        </Card.Body>
      </Card>
    </Link>
  );
};