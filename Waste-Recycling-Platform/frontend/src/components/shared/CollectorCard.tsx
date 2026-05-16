import React from "react";
import { Card, Badge, Button, Avatar } from "@/components/ui";
import Link from "next/link";

export interface CollectorCardProps {
  id: string;
  name: string;
  avatar?: string;
  rating: number;
  completedTasks: number;
  reviews: number;
  location: string;
  status: "available" | "busy" | "offline";
  responseTime?: string;
  onContactClick?: () => void;
}

const statusColors = {
  available: "success",
  busy: "warning",
  offline: "default",
} as const;

export const CollectorCard: React.FC<CollectorCardProps> = ({
  id,
  name,
  avatar,
  rating,
  completedTasks,
  reviews,
  location,
  status,
  responseTime,
  onContactClick,
}) => {
  return (
    <Link href={`/collectors/${id}`}>
      <Card hoverable className="h-full cursor-pointer">
        <Card.Body className="space-y-4">
          {/* Header */}
          <div className="flex items-start justify-between gap-3">
            <div className="flex items-start gap-3 flex-1">
              <Avatar
                src={avatar}
                initials={name
                  .split(" ")
                  .map((n) => n[0])
                  .join("")}
                size="lg"
              />
              <div className="flex-1">
                <h3 className="text-lg font-semibold text-gray-900">{name}</h3>
                <p className="text-sm text-gray-600">{location}</p>
              </div>
            </div>
            <Badge variant={statusColors[status]}>
              {status.charAt(0).toUpperCase() + status.slice(1)}
            </Badge>
          </div>

          {/* Rating */}
          <div className="flex items-center gap-2">
            <div className="flex gap-1">
              {[...Array(5)].map((_, i) => (
                <span
                  key={i}
                  className={`text-xl ${i < Math.round(rating) ? "⭐" : "☆"}`}
                />
              ))}
            </div>
            <p className="text-sm font-medium text-gray-700">
              {rating.toFixed(1)} ({reviews} reviews)
            </p>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-2 gap-3 py-3 border-t border-b border-gray-200">
            <div className="text-center">
              <p className="text-2xl font-bold text-amber-600">
                {completedTasks}
              </p>
              <p className="text-xs text-gray-600">Tasks Completed</p>
            </div>
            {responseTime && (
              <div className="text-center">
                <p className="text-sm font-medium text-gray-900">
                  {responseTime}
                </p>
                <p className="text-xs text-gray-600">Response Time</p>
              </div>
            )}
          </div>

          {/* Action Button */}
          <Button
            variant="primary"
            size="sm"
            className="w-full"
            onClick={(e) => {
              e.preventDefault();
              onContactClick?.();
            }}
          >
            Contact Collector
          </Button>
        </Card.Body>
      </Card>
    </Link>
  );
};
