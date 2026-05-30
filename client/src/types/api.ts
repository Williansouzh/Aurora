// Enums
export enum LifeArea { Health=1,Work=2,Studies=3,Money=4,Relationships=5,Home=6,Leisure=7,Spirituality=8,Projects=9 }
export enum GoalStatus { Active=1,Paused=2,Completed=3,Cancelled=4 }
export enum GoalMetricType { None=0,Numeric=1,Percentage=2 }
export enum DailyTaskPriority { Low=1,Medium=2,High=3 }
export enum DailyTaskStatus { Pending=1,Completed=2,Overdue=3 }
export enum HabitFrequencyType { Daily=1,Weekly=2 }
export enum HabitDifficulty { Easy=1,Medium=2,Hard=3 }
export enum HabitCheckInStatus { Done=1,Skipped=2 }
export enum TimelineEventType { HabitCheckedIn=1,TaskCompleted=2,GoalProgressed=3,GoalCompleted=4,DiaryWritten=5,EvolutionPhotoAdded=6,WeeklyReviewClosed=7,MonthlyBudgetClosed=8,AchievementUnlocked=9,ManualPost=10 }
export enum WeeklyPlanStatus { NotStarted=1,InProgress=2,Closed=3 }

// Auth
export interface User { id: string; name: string; email: string; level: number; totalXp: number; achievements: string[] }

// Tasks
export interface DailyTask { id: string; title: string; notes?: string; priority: DailyTaskPriority; status: DailyTaskStatus; date: string; completedAt?: string; isBacklog: boolean }
export interface TodayResponse { pending: DailyTask[]; completed: DailyTask[]; overdue: DailyTask[] }

// Habits
export interface Habit { id: string; name: string; description?: string; area: LifeArea; frequencyType: HabitFrequencyType; daysOfWeek: number[]; timesPerWeek: number; difficulty: HabitDifficulty; xpReward: number; currentStreak: number; bestStreak: number; isActive: boolean; createdAt: string }
export interface HabitCheckIn { id: string; habitId: string; date: string; status: HabitCheckInStatus; note?: string; xpGenerated: number; createdAt: string }
export interface CheckInDay { date: string; status?: HabitCheckInStatus }
export interface HabitStats { currentStreak: number; bestStreak: number; doneThisMonth: number; totalThisMonth: number; calendar: CheckInDay[] }

// Goals
export interface Milestone { id: string; title: string; isRequired: boolean; isCompleted: boolean; completedAt?: string }
export interface Goal { id: string; title: string; description?: string; area: LifeArea; status: GoalStatus; startDate?: string; targetDate?: string; metricType: GoalMetricType; targetValue: number; currentValue: number; progress: number; coverImage?: string; linkedCategoryId?: string; milestones: Milestone[]; createdAt: string }

// Timeline
export interface TimelineEvent { id: string; type: TimelineEventType; area?: LifeArea; title: string; description?: string; occurredAt: string; sourceModule?: string; mediaUrls: string[]; isHidden: boolean; isFavorite: boolean; createdAt: string }

// Diary
export interface DiaryEntry { id: string; date: string; content: string; mood: number; tags: string[]; photos: string[]; isPrivate: boolean; createdAt: string; updatedAt: string }

// Evolution
export interface EvolutionAlbum { id: string; title: string; area: LifeArea; description?: string; coverImage?: string; isPrivate: boolean; createdAt: string }
export interface EvolutionPhoto { id: string; albumId: string; imageUrl: string; caption?: string; date: string; tags: string[]; linkedGoalId?: string; linkedHabitId?: string; createdAt: string }

// Weekly Plan
export interface WeeklyPlan { id: string; weekStart: string; weekEnd: string; mainFocus?: string; linkedGoalIds: string[]; priorities: string[]; notes?: string; status: WeeklyPlanStatus; review?: string; xpGenerated: number; closedAt?: string; createdAt: string }

// Home
export interface HabitTodayDto { id: string; name: string; area: LifeArea; currentStreak: number; xpReward: number; checkedInToday: boolean }
export interface MoodHistoryDto { date: string; mood: number }
export interface HomeData { pendingTasksCount: number; completedTasksCount: number; topPendingTasks: DailyTask[]; todayHabits: HabitTodayDto[]; featuredGoals: Goal[]; recentEvents: TimelineEvent[]; todayMood?: number; moodHistory: MoodHistoryDto[]; totalBalance: number; monthlyIncome: number; monthlyExpense: number; totalXp: number; level: number; levelName: string; xpToNextLevel: number; achievements: string[] }

// Pagination
export interface PagedResult<T> { items: T[]; totalCount: number; page: number; pageSize: number; totalPages: number }

// API Response wrapper
export interface ApiResponse<T> { success: boolean; data: T; message?: string }

// Retrospectives
export interface TopHabitDto { habitName: string; checkIns: number }
export interface WeeklyRetrospective { weekStart: string; weekEnd: string; tasksCompleted: number; habitCheckIns: number; habitsWithStreak: number; averageMood: number; topHabits: TopHabitDto[]; xpEarned: number; weeklyReview?: string }
export interface MonthlyRetrospective { month: number; year: number; tasksCompleted: number; habitCheckIns: number; newGoalsCreated: number; goalsCompleted: number; averageMood: number; moodEntriesCount: number; diaryEntriesCount: number; monthlyIncome: number; monthlyExpense: number; monthlySavings: number; xpEarned: number; unlockedAchievements: string[] }
