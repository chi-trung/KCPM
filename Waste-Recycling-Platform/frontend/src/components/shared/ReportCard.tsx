import React from "react";
import { Card, Badge, Button } from "@/components/ui";
import Link from "next/link";

export interface ReportCardProps {
  id: string;
  title: string;
  description: string;
  location: string;
  wasteType: string;
  status: "pending" | "assigned" | "completed" | "cancelled";
  image?: string;
  createdAt: string;
  points?: number;
  onActionClick?: () => void;
  actionButtonLabel?: string;
}

const statusColors = {
  pending: "warning",
  assigned: "info",
  completed: "success",
  cancelled: "danger",
} as const;

const statusLabels = {
  pending: "Pending",
  assigned: "Assigned",
  completed: "Completed",
  cancelled: "Cancelled",
};

export const ReportCard: React.FC<ReportCardProps> = ({
  id,
  title,
  description,
  location,
  wasteType,
  status,
  image,
  createdAt,
  points,
  onActionClick,
  actionButtonLabel = "View Details",
}) => {
  return (
    <Link href={`/reports/${id}`}>
      <Card hoverable className="h-full cursor-pointer">
        {/* Image Container */}
        {image && (
          <div className="w-full h-48 overflow-hidden bg-gray-200">
            <img
              src={image}
              alt={title}
              className="w-full h-full object-cover hover:scale-105 transition-transform"
            />
          </div>
        )}

        <Card.Body className="space-y-3">
          {/* Header */}
          <div className="flex items-start justify-between gap-2">
            <h3 className="text-lg font-semibold text-gray-900 flex-1">
              {title}
            </h3>
            <Badge variant={statusColors[status]}>{statusLabels[status]}</Badge>
          </div>

          {/* Description */}
          <p className="text-sm text-gray-600 line-clamp-2">{description}</p>

          {/* Info Grid */}
          <div className="grid grid-cols-2 gap-3 py-3 border-t border-b border-gray-200">
            <div>
              <p className="text-xs text-gray-500 uppercase">Waste Type</p>
              <p className="text-sm font-medium text-gray-900">{wasteType}</p>
            </div>
            <div>
              <p className="text-xs text-gray-500 uppercase">Location</p>
              <p className="text-sm font-medium text-gray-900 truncate">
                {location}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500 uppercase">Date</p>
              <p className="text-sm font-medium text-gray-900">
                {new Date(createdAt).toLocaleDateString()}
              </p>
            </div>
            {points && (
              <div>
                <p className="text-xs text-gray-500 uppercase">Points</p>
                <p className="text-sm font-bold text-amber-600">+{points}</p>
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