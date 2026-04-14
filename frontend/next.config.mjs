const nextConfig = {
  images: {
    remotePatterns: [
      {
        protocol: "http",
        hostname: "localhost",
        port: "5047",
        pathname: "/**",
      },
    ],
  },
};

export default nextConfig;
