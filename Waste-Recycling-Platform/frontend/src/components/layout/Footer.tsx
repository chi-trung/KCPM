import React from "react";
import Link from "next/link";

interface FooterProps {
  companyName?: string;
  year?: number;
  links?: FooterLink[];
  social?: SocialLink[];
}

interface FooterLink {
  label: string;
  href: string;
}

interface SocialLink {
  name: string;
  href: string;
  icon: React.ReactNode;
}

export const Footer: React.FC<FooterProps> = ({
  companyName = "Waste Recycling Platform",
  year = new Date().getFullYear(),
  links = [],
  social = [],
}) => {
  return (
    <footer className="bg-gray-900 text-white mt-auto">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
          {/* Brand */}
          <div>
            <h3 className="text-xl font-bold text-amber-500 mb-4">
              {companyName}
            </h3>
            <p className="text-gray-400 text-sm">
              Making waste collection and recycling easier for everyone.
            </p>
          </div>

          {/* Quick Links */}
          {links.length > 0 && (
            <div>
              <h4 className="font-semibold mb-4">Quick Links</h4>
              <ul className="space-y-2">
                {links.map((link) => (
                  <li key={link.href}>
                    <Link
                      href={link.href}
                      className="text-gray-400 hover:text-amber-500 transition-colors text-sm"
                    >
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Company Info */}
          <div>
            <h4 className="font-semibold mb-4">Company</h4>
            <ul className="space-y-2 text-gray-400 text-sm">
              <li>
                <Link href="/" className="hover:text-amber-500">
                  About Us
                </Link>
              </li>
              <li>
                <Link href="/" className="hover:text-amber-500">
                  Contact
                </Link>
              </li>
              <li>
                <Link href="/" className="hover:text-amber-500">
                  Privacy Policy
                </Link>
              </li>
              <li>
                <Link href="/" className="hover:text-amber-500">
                  Terms
                </Link>
              </li>
            </ul>
          </div>

          {/* Social Links */}
          {social.length > 0 && (
            <div>
              <h4 className="font-semibold mb-4">Follow Us</h4>
              <div className="flex gap-4">
                {social.map((link) => (
                  <Link
                    key={link.name}
                    href={link.href}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-gray-400 hover:text-amber-500 transition-colors"
                    aria-label={link.name}
                  >
                    {link.icon}
                  </Link>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Divider */}
        <div className="border-t border-gray-800"></div>

        {/* Copyright */}
        <div className="pt-8 flex flex-col md:flex-row justify-between items-center">
          <p className="text-gray-400 text-sm">
            &copy; {year} {companyName}. All rights reserved.
          </p>
          <p className="text-gray-400 text-sm mt-4 md:mt-0">
            Building a sustainable future 🌱
          </p>
        </div>
      </div>
    </footer>
  );
};
