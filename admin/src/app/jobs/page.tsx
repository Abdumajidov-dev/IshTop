"use client";

import { useEffect, useState } from "react";
import AdminLayout from "@/components/layout/AdminLayout";
import { api } from "@/lib/api";
import type { Job, PaginatedResponse } from "@/types";
import { ExperienceLabels, WorkTypeLabels } from "@/types";

export default function JobsPage() {
  const [data, setData] = useState<PaginatedResponse<Job> | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const fetchJobs = () => {
    setLoading(true);
    api
      .getJobs(page)
      .then(setData)
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(fetchJobs, [page]);

  const handleModerate = async (job: Job, action: string) => {
    try {
      if (action === "delete") {
        await api.deleteJob(job.id);
      } else {
        await api.moderateJob(job.id, {
          isActive: action === "activate" ? true : action === "deactivate" ? false : job.isActive,
          isSpam: action === "spam" ? true : false,
          isFeatured: action === "feature" ? !job.isFeatured : job.isFeatured,
        });
      }
      fetchJobs();
    } catch (error) {
      console.error("Moderation error:", error);
    }
  };

  return (
    <AdminLayout>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Vakansiyalar</h1>
        <p className="text-gray-500">Jami: {data?.total ?? 0} ta vakansiya</p>
      </div>

      <div className="space-y-4">
        {loading ? (
          <div className="flex justify-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
          </div>
        ) : (
          data?.items.map((job) => (
            <div
              key={job.id}
              className="bg-white rounded-xl shadow-sm border border-gray-100 p-6"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    {job.isFeatured && (
                      <span className="text-yellow-500">⭐</span>
                    )}
                    <h3 className="text-lg font-semibold text-gray-800">
                      {job.title}
                    </h3>
                    {job.isSpam && (
                      <span className="px-2 py-0.5 bg-red-100 text-red-600 rounded-full text-xs">
                        SPAM
                      </span>
                    )}
                    {!job.isActive && (
                      <span className="px-2 py-0.5 bg-gray-100 text-gray-500 rounded-full text-xs">
                        Nofaol
                      </span>
                    )}
                  </div>
                  {job.company && (
                    <p className="text-gray-600 mt-1">🏢 {job.company}</p>
                  )}

                  <div className="flex flex-wrap gap-2 mt-3">
                    {job.techStacks.map((tech) => (
                      <span
                        key={tech}
                        className="px-2 py-1 bg-blue-50 text-blue-600 rounded text-xs"
                      >
                        {tech}
                      </span>
                    ))}
                  </div>

                  <div className="flex gap-4 mt-3 text-sm text-gray-500">
                    {job.experienceLevel !== null && (
                      <span>
                        📊 {ExperienceLabels[job.experienceLevel] ?? "—"}
                      </span>
                    )}
                    {job.workType !== null && (
                      <span>🏠 {WorkTypeLabels[job.workType] ?? "—"}</span>
                    )}
                    {job.salaryMin && (
                      <span>
                        💰 {job.salaryMin}-{job.salaryMax}{" "}
                        {job.currency === 1 ? "USD" : "UZS"}
                      </span>
                    )}
                    {job.location && <span>📍 {job.location}</span>}
                  </div>
                </div>

                <div className="flex gap-2 ml-4">
                  <button
                    onClick={() =>
                      handleModerate(
                        job,
                        job.isActive ? "deactivate" : "activate"
                      )
                    }
                    className={`px-3 py-1 rounded text-xs ${
                      job.isActive
                        ? "bg-yellow-100 text-yellow-700"
                        : "bg-green-100 text-green-700"
                    }`}
                  >
                    {job.isActive ? "O&apos;chirish" : "Faollashtirish"}
                  </button>
                  <button
                    onClick={() => handleModerate(job, "feature")}
                    className="px-3 py-1 rounded text-xs bg-purple-100 text-purple-700"
                  >
                    {job.isFeatured ? "⭐ Olib tashlash" : "⭐ Featured"}
                  </button>
                  <button
                    onClick={() => handleModerate(job, "delete")}
                    className="px-3 py-1 rounded text-xs bg-red-100 text-red-700"
                  >
                    O&apos;chirish
                  </button>
                </div>
              </div>

              <p className="text-sm text-gray-400 mt-3">
                {new Date(job.createdAt).toLocaleString("uz-UZ")}
              </p>
            </div>
          ))
        )}
      </div>

      {data && data.total > 20 && (
        <div className="mt-6 flex items-center justify-center gap-4">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="px-4 py-2 bg-white rounded-lg shadow-sm disabled:opacity-50"
          >
            Oldingi
          </button>
          <span className="text-gray-500">
            {page} / {Math.ceil(data.total / 20)}
          </span>
          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={page >= Math.ceil(data.total / 20)}
            className="px-4 py-2 bg-white rounded-lg shadow-sm disabled:opacity-50"
          >
            Keyingi
          </button>
        </div>
      )}
    </AdminLayout>
  );
}
