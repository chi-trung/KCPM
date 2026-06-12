/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
};

// Only use standalone output for Docker builds, not Vercel
if (process.env.DOCKER_BUILD === 'true') {
  nextConfig.output = 'standalone';
}

module.exports = nextConfig;
