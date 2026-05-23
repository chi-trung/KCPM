import React from "react";
import { Card, Badge, Button } from "@/components/ui";
import Link from "next/link";

export interface EnterpriseCardProps {
  id: string;
  name: string;
  logo?: string;
  description: string;
  serviceArea: string;
  status: "active" | "inactive" | "pending";
  tasksPosted: number;
  rating?: number;
  contactEmail: string;
  contactPhone: string;
  onContactClick?: () => void;
}

const statusColors = {
  active: "success",
  inactive: "default",
  pending: "warning",
} as const;

export const EnterpriseCard: React.FC<EnterpriseCardProps> = ({
  id,
  name,
  logo,
  description,
  serviceArea,
  status,
  tasksPosted,
  rating,
  contactEmail,
  contactPhone,
  onContactClick,
}) => {
  return (
    <Link href={`/enterprises/${id}`}>
      <Card hoverable className="h-full cursor-pointer">
        {/* Logo */}
        {logo && (
          <div className="w-full h-32 overflow-hidden bg-gray-100 flex items-center justify-center">
            <img
              src={logo}
              alt={name}
              className="h-20 object-contain hover:scale-110 transition-transform"
            />
          </div>
        )}

        <Card.Body className="space-y-3">
          {/* Header */}
          <div className="flex items-start justify-between gap-2">
            <h3 className="text-lg font-semibold text-gray-900 flex-1">
              {name}
            </h3>
            <Badge variant={statusColors[status]}>
              {status.charAt(0).toUpperCase() + status.slice(1)}
            </Badge>
          </div>

          {/* Description */}
          <p className="text-sm text-gray-600 line-clamp-2">{description}</p>

          {/* Service Area & Rating */}
          <div className="flex items-center justify-between py-2 px-3 bg-amber-50 rounded-lg">
            <div>
              <p className="text-xs text-gray-600">Service Area</p>
              <p className="text-sm font-medium text-gray-900">{serviceArea}</p>
            </div>
            {rating && (
              <div className="text-right">
                <p className="text-sm font-bold text-amber-600">⭐ {rating}</p>
              </div>
            )}
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-2 gap-3 py-3 border-t border-b border-gray-200">
            <div>
              <p className="text-xs text-gray-500 uppercase">Tasks Posted</p>
              <p className="text-2xl font-bold text-amber-600">{tasksPosted}</p>
            </div>
            <div>
              <p className="text-xs text-gray-500 uppercase">Contact</p>
              <p className="text-xs font-medium text-gray-900 truncate">
                {contactEmail}
              </p>
            </div>
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
            Contact Enterprise
          </Button>
        </Card.Body>
      </Card>
    </Link>
  );
};
