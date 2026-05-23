import React from "react";

interface CardProps {
  children: React.ReactNode;
  className?: string;
  hoverable?: boolean;
}

interface CardHeaderProps {
  children: React.ReactNode;
  className?: string;
}

interface CardBodyProps {
  children: React.ReactNode;
  className?: string;
}

interface CardFooterProps {
  children: React.ReactNode;
  className?: string;
}

const CardHeader: React.FC<CardHeaderProps> = ({ children, className }) => (
  <div className={`px-6 py-4 border-b border-gray-200 ${className || ""}`}>
    {children}
  </div>
);

const CardBody: React.FC<CardBodyProps> = ({ children, className }) => (
  <div className={`px-6 py-4 ${className || ""}`}>{children}</div>
);

const CardFooter: React.FC<CardFooterProps> = ({ children, className }) => (
  <div
    className={`px-6 py-4 border-t border-gray-200 bg-gray-50 ${className || ""}`}
  >
    {children}
  </div>
);

export const Card: React.FC<CardProps> & {
  Header: typeof CardHeader;
  Body: typeof CardBody;
  Footer: typeof CardFooter;
} = ({ children, className, hoverable = false }) => {
  return (
    <div
      className={`
      bg-white rounded-lg shadow-md
      ${hoverable ? "hover:shadow-lg transition-shadow duration-200" : ""}
      ${className || ""}
    `}
    >
      {children}
    </div>
  );
};

Card.Header = CardHeader;
Card.Body = CardBody;
Card.Footer = CardFooter;
