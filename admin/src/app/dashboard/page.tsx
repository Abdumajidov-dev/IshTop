"use client";

import { useEffect, useState } from "react";
import AdminLayout from "@/components/layout/AdminLayout";
import { api } from "@/lib/api";
import type { DashboardStats } from "@/types";

export default function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .getDashboardStats()
      .then(setStats)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const cards = stats
    ? [
        { label: "Jami foydalanuvchilar", value: stats.totalUsers, icon: "👥", color: "bg-blue-500" },
        { label: "Aktiv foydalanuvchilar", value: stats.activeUsers, icon: "✅", color: "bg-green-500" },
        { label: "Jami vakansiyalar", value: stats.totalJobs, icon: "💼", color: "bg-purple-500" },
        { label: "Aktiv vakansiyalar", value: stats.activeJobs, icon: "📋", color: "bg-orange-500" },
        { label: "Arizalar", value: stats.totalApplications, icon: "📨", color: "bg-pink-500" },
        { label: "Kanallar", value: stats.totalChannels, icon: "📡", color: "bg-teal-500" },
      ]
    : [];

  return (
    <AdminLayout>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Dashboard</h1>
        <p className="text-gray-500">IshTop platformasi statistikasi</p>
      </div>

      {loading ? (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {cards.map((card) => (
            <div
              key={card.label}
              className="bg-white rounded-xl shadow-sm p-6 border border-gray-100"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">{card.label}</p>
                  <p className="text-3xl font-bold text-gray-800 mt-1">
                    {card.value.toLocaleString()}
                  </p>
                </div>
                <div
                  className={`${card.color} text-white w-12 h-12 rounded-lg flex items-center justify-center text-xl`}
                >
                  {card.icon}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </AdminLayout>
  );
}
