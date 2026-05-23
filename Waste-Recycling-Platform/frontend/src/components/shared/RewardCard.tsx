import React from "react";
import { Card, Badge, Button, Progress } from "@/components/ui";
import Link from "next/link";

export interface RewardCardProps {
  id: string;
  name: string;
  description: string;
  points: number;
  currentPoints?: number;
  image?: string;
  category?: string;
  available?: boolean;
  stock?: number;
  onRedeemClick?: () => void;
}

export const RewardCard: React.FC<RewardCardProps> = ({
  id,
  name,
  description,
  points,
  currentPoints = 0,
  image,
  category,
  available = true,
  stock,
  onRedeemClick,
}) => {
  const canRedeem = currentPoints >= points && available;

  return (
    <Link href={`/rewards/${id}`}>
      <Card hoverable className="h-full cursor-pointer">
        {/* Image Container */}
        {image && (
          <div className="w-full h-40 overflow-hidden bg-gray-200">
            <img
              src={image}
              alt={name}
              className="w-full h-full object-cover hover:scale-105 transition-transform"
            />
          </div>
        )}

        <Card.Body className="space-y-3">
          {/* Header */}
          <div className="flex items-start justify-between gap-2">
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-gray-900">{name}</h3>
              {category && (
                <p className="text-xs text-gray-500 mt-1">{category}</p>
              )}
            </div>
            {!available && <Badge variant="danger">Out of Stock</Badge>}
          </div>

          {/* Description */}
          <p className="text-sm text-gray-600 line-clamp-2">{description}</p>

          {/* Points Required */}
          <div className="bg-amber-50 rounded-lg p-3 border border-amber-200">
            <p className="text-xs text-gray-600 mb-2">
              Points Required:{" "}
              <span className="font-bold text-amber-700">{points}</span>
            </p>
            <Progress
              value={Math.min(currentPoints, points)}
              max={points}
              color="amber"
            />
            <p className="text-xs text-gray-600 mt-2">
              You have:{" "}
              <span className="font-bold text-green-600">{currentPoints}</span>
            </p>
          </div>

          {/* Stock Info */}
          {stock !== undefined && (
            <p className="text-xs text-gray-600 text-center">
              {stock > 0 ? `${stock} items available` : "Out of stock"}
            </p>
          )}

          {/* Redeem Button */}
          <Button
            variant={canRedeem ? "success" : "secondary"}
            size="sm"
            className="w-full"
            disabled={!canRedeem}
            onClick={(e: React.MouseEvent) => {
              e.preventDefault();
              onRedeemClick?.();
            }}
          >
            {canRedeem ? "Redeem Now" : "Not Enough Points"}
          </Button>
        </Card.Body>
      </Card>
    </Link>
  );
};