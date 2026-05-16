/** @type {import('next').NextConfig} */
module.exports = {
  reactStrictMode: true,
  // Required for Docker: bundles a minimal server in .next/standalone
  output: 'standalone',
};
