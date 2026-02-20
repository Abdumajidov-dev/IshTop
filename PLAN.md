# IshTop Project Implementation Plan

Based on the Master Prompt requirements, here is the roadmap for building the IshTop platform.

## Phase 1: Foundation & Design (Current Focus)
- [x] **1. System Architecture Design**
  - [x] Microservices vs Modular Monolith decision
  - [x] Technology stack confirmation
  - [x] Infrastructure setup (Docker, etc.)
- [x] **2. Database Schema Design**
  - [x] Users & Profiles
  - [x] Jobs & Companies
  - [x] Applications & Matches
  - [x] Subscriptions & Payments
- [ ] **3. AI Strategy & Prompts**
  - [ ] Resume parsing prompt (Uzbek)
  - [ ] Job extraction prompt
  - [ ] Matching logic

## Phase 2: Core Backend Implementation
- [ ] **4. API Development (.NET 8)**
  - [ ] Setup Solution & Layers (Clean Architecture)
  - [ ] Implement Authentication (JWT, Telegram Auth)
  - [ ] CRUD for Users, Jobs
  - [ ] Job Matching Engine
- [ ] **5. Telegram Bot Implementation**
  - [ ] User Onboarding Flow
  - [ ] Profile Management
  - [ ] Job Browsing & Search
  - [ ] Notifications
- [ ] **6. Parser Service**
  - [ ] Telegram Channel Listener
  - [ ] Spam Filter
  - [ ] AI Extraction Pipeline

## Phase 3: Admin & Analytics
- [ ] **7. Admin Dashboard (Next.js)**
  - [ ] User Management
  - [ ] Job Moderation
  - [ ] Analytics Dashboard
  - [ ] Broadcast Tools

## Phase 4: Launch & Growth
- [ ] **8. Monetization & Payments**
  - [ ] Subscription Logic
  - [ ] Payment Gateway Integration
- [ ] **9. Deployment & Scaling**
  - [ ] CI/CD Pipeline
  - [ ] Server Setup (VPS/Cloud)
  - [ ] Monitoring & Logging
