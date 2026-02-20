"use client";

import { useEffect, useState } from "react";
import AdminLayout from "@/components/layout/AdminLayout";
import { api } from "@/lib/api";
import type { Channel } from "@/types";

export default function ChannelsPage() {
  const [channels, setChannels] = useState<Channel[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAdd, setShowAdd] = useState(false);
  const [newChannel, setNewChannel] = useState({ telegramId: "", title: "", username: "" });

  const fetchChannels = () => {
    setLoading(true);
    api
      .getChannels()
      .then(setChannels)
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(fetchChannels, []);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.addChannel({
        telegramId: parseInt(newChannel.telegramId),
        title: newChannel.title,
        username: newChannel.username || undefined,
      });
      setShowAdd(false);
      setNewChannel({ telegramId: "", title: "", username: "" });
      fetchChannels();
    } catch (error) {
      console.error("Add channel error:", error);
    }
  };

  const handleToggle = async (id: string) => {
    await api.toggleChannel(id);
    fetchChannels();
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Kanalni o'chirishni tasdiqlaysizmi?")) return;
    await api.deleteChannel(id);
    fetchChannels();
  };

  return (
    <AdminLayout>
      <div className="mb-6 flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Kanallar</h1>
          <p className="text-gray-500">{channels.length} ta kanal</p>
        </div>
        <button
          onClick={() => setShowAdd(!showAdd)}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
        >
          + Kanal qo&apos;shish
        </button>
      </div>

      {showAdd && (
        <form
          onSubmit={handleAdd}
          className="bg-white rounded-xl shadow-sm border p-6 mb-6 space-y-4"
        >
          <div className="grid grid-cols-3 gap-4">
            <input
              type="number"
              placeholder="Telegram ID"
              value={newChannel.telegramId}
              onChange={(e) =>
                setNewChannel({ ...newChannel, telegramId: e.target.value })
              }
              className="px-4 py-2 border rounded-lg"
              required
            />
            <input
              type="text"
              placeholder="Kanal nomi"
              value={newChannel.title}
              onChange={(e) =>
                setNewChannel({ ...newChannel, title: e.target.value })
              }
              className="px-4 py-2 border rounded-lg"
              required
            />
            <input
              type="text"
              placeholder="@username (ixtiyoriy)"
              value={newChannel.username}
              onChange={(e) =>
                setNewChannel({ ...newChannel, username: e.target.value })
              }
              className="px-4 py-2 border rounded-lg"
            />
          </div>
          <div className="flex gap-2">
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded-lg"
            >
              Qo&apos;shish
            </button>
            <button
              type="button"
              onClick={() => setShowAdd(false)}
              className="px-4 py-2 bg-gray-100 rounded-lg"
            >
              Bekor qilish
            </button>
          </div>
        </form>
      )}

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Kanal
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Telegram ID
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Ishlar soni
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Holat
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Oxirgi parse
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                Amallar
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {loading ? (
              <tr>
                <td colSpan={6} className="px-6 py-12 text-center text-gray-400">
                  Yuklanmoqda...
                </td>
              </tr>
            ) : (
              channels.map((channel) => (
                <tr key={channel.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4">
                    <p className="font-medium text-gray-800">{channel.title}</p>
                    {channel.username && (
                      <p className="text-sm text-gray-500">
                        @{channel.username}
                      </p>
                    )}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600">
                    {channel.telegramId}
                  </td>
                  <td className="px-6 py-4 text-sm font-medium text-gray-800">
                    {channel.jobCount}
                  </td>
                  <td className="px-6 py-4">
                    <span
                      className={`px-2 py-1 rounded-full text-xs font-medium ${
                        channel.isActive
                          ? "bg-green-100 text-green-700"
                          : "bg-red-100 text-red-700"
                      }`}
                    >
                      {channel.isActive ? "Faol" : "Nofaol"}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600">
                    {channel.lastParsedAt
                      ? new Date(channel.lastParsedAt).toLocaleString("uz-UZ")
                      : "—"}
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex gap-2">
                      <button
                        onClick={() => handleToggle(channel.id)}
                        className={`px-3 py-1 rounded text-xs ${
                          channel.isActive
                            ? "bg-yellow-100 text-yellow-700"
                            : "bg-green-100 text-green-700"
                        }`}
                      >
                        {channel.isActive ? "To'xtatish" : "Yoqish"}
                      </button>
                      <button
                        onClick={() => handleDelete(channel.id)}
                        className="px-3 py-1 rounded text-xs bg-red-100 text-red-700"
                      >
                        O&apos;chirish
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </AdminLayout>
  );
}
