import React from "react";
import { Card, Badge } from "@/components/ui";

export interface UserProfileCardProps {
  name: string;
  avatar?: string;
  role: "citizen" | "collector" | "enterprise" | "admin";
  email: string;
  phone?: string;
  joinedDate: string;
  stats: {
    label: string;
    value: number | string;
  }[];
  badges?: string[];
  verified?: boolean;
}

const roleStyles = {
  citizen: "bg-blue-100 text-blue-800",
  collector: "bg-green-100 text-green-800",
  enterprise: "bg-amber-100 text-amber-800",
  admin: "bg-red-100 text-red-800",
};

export const UserProfileCard: React.FC<UserProfileCardProps> = ({
  name,
  avatar,
  role,
  email,
  phone,
  joinedDate,
  stats,
  badges,
  verified,
}) => {
  return (
    <Card className="border-2 border-amber-200">
      <Card.Body className="space-y-4">
        {/* Header */}
        <div className="flex items-start gap-4">
          {avatar ? (
            <img
              src={avatar}
              alt={name}
              className="w-16 h-16 rounded-full object-cover"
            />
          ) : (
            <div className="w-16 h-16 rounded-full bg-amber-200 flex items-center justify-center text-xl font-bold">
              {name
                .split(" ")
                .map((n) => n[0])
                .join("")}
            </div>
          )}
          <div className="flex-1">
            <div className="flex items-center gap-2 mb-2">
              <h3 className="text-lg font-bold text-gray-900">{name}</h3>
              {verified && <span className="text-lg">✓</span>}
            </div>
            <Badge
              variant={
                role === "citizen"
                  ? "info"
                  : role === "collector"
                    ? "success"
                    : role === "enterprise"
                      ? "warning"
                      : "danger"
              }
            >
              {role.charAt(0).toUpperCase() + role.slice(1)}
            </Badge>
          </div>
        </div>

        {/* Contact Info */}
        <div className="space-y-2 py-3 border-t border-b border-gray-200">
          <p className="text-sm">
            <span className="text-gray-600">Email:</span>{" "}
            <span className="font-medium">{email}</span>
          </p>
          {phone && (
            <p className="text-sm">
              <span className="text-gray-600">Phone:</span>{" "}
              <span className="font-medium">{phone}</span>
            </p>
          )}
          <p className="text-sm">
            <span className="text-gray-600">Joined:</span>{" "}
            <span className="font-medium">
              {new Date(joinedDate).toLocaleDateString()}
            </span>
          </p>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-3 gap-2">
          {stats.map((stat, index) => (
            <div key={index} className="text-center p-2 bg-gray-50 rounded">
              <p className="text-2xl font-bold text-amber-600">{stat.value}</p>
              <p className="text-xs text-gray-600">{stat.label}</p>
            </div>
          ))}
        </div>

        {/* Badges/Achievements */}
        {badges && badges.length > 0 && (
          <div className="pt-2 border-t border-gray-200">
            <p className="text-xs text-gray-600 mb-2">Achievements</p>
            <div className="flex flex-wrap gap-2">
              {badges.map((badge, index) => (
                <Badge key={index} variant="success" size="sm">
                  {badge}
                </Badge>
              ))}
            </div>
          </div>
        )}
      </Card.Body>
    </Card>
  );
};
