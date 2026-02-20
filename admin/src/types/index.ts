export interface User {
  id: string;
  telegramId: number;
  username: string | null;
  firstName: string | null;
  lastName: string | null;
  language: number;
  state: number;
  createdAt: string;
  notificationsEnabled: boolean;
  profile?: UserProfile;
}

export interface UserProfile {
  techStacks: string[];
  experienceLevel: number;
  salaryMin: number | null;
  salaryMax: number | null;
  currency: number;
  workType: number;
  city: string | null;
  englishLevel: number;
  isComplete: boolean;
}

export interface Job {
  id: string;
  title: string;
  description: string;
  company: string | null;
  techStacks: string[];
  experienceLevel: number | null;
  salaryMin: number | null;
  salaryMax: number | null;
  currency: number | null;
  workType: number | null;
  location: string | null;
  contactInfo: string | null;
  isActive: boolean;
  isSpam: boolean;
  isFeatured: boolean;
  createdAt: string;
}

export interface Channel {
  id: string;
  telegramId: number;
  title: string;
  username: string | null;
  isActive: boolean;
  jobCount: number;
  lastParsedAt: string | null;
  createdAt: string;
}

export interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  totalJobs: number;
  activeJobs: number;
  totalApplications: number;
  totalChannels: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export const ExperienceLabels: Record<number, string> = {
  0: "Intern",
  1: "Junior",
  2: "Middle",
  3: "Senior",
  4: "Lead",
};

export const WorkTypeLabels: Record<number, string> = {
  0: "Remote",
  1: "Office",
  2: "Hybrid",
};

export const UserStateLabels: Record<number, string> = {
  0: "New",
  1: "Onboarding",
  2: "Active",
  3: "Inactive",
};
