export type Role = 'GM' | 'FIELD' | 'SALES' | 'OPS' | 'STORE' | 'FINANCE' | 'SUPERVISOR';

export interface User {
  id: number;
  email: string;
  role: Role;
  access?: Record<string, { read: boolean; write: boolean }>;
}

export interface Customer {
  id: number;
  number?: string;
  name: string;
  phone: string;
  email: string;
  address: string;
  notes: string;
  active: boolean;
  createdAt: string;
}

export interface Staff {
  id: number;
  number?: string;
  name: string;
  phone: string;
  position: string;
  active: boolean;
  createdAt: string;
}

export interface Team {
  id: number;
  name: string;
  supervisorStaffId?: number;
  supervisorName?: string;
  memberCount: number;
  active: boolean;
  createdAt: string;
}

export interface QuotationLine {
  id?: number;
  description: string;
  qty: number;
  unitPrice: number;
  days: number;
  location: string;
}

export interface Quotation {
  id: number;
  number: string;
  customerId: number;
  customerName?: string;
  subtotal: number;
  discountPct: number;
  total: number;
  status: 'DRAFT' | 'PENDING_APPROVAL' | 'APPROVED' | 'ACCEPTED' | 'REJECTED' | 'CANCELLED';
  paymentTerms: string;
  serviceAddress: string;
  basis: string;
  notes: string;
  createdBy: string;
  approvedBy?: string;
  cancelRequested: boolean;
  cancelReason?: string;
  lines: QuotationLine[];
  createdAt: string;
}

export interface Job {
  id: number;
  number: string;
  customerId: number;
  customerName?: string;
  quotationId?: number;
  serviceType: string;
  address: string;
  scheduledFor: string;
  startTime?: string;
  durationHours?: number;
  notes: string;
  problemNotes: string;
  endDate?: string;
  teamId?: number;
  cancelRequested: boolean;
  cancelReason?: string;
  amount: number;
  status: 'PLANNED' | 'IN_PROGRESS' | 'COMPLETED' | 'PROBLEM' | 'CANCELLED';
  staffCount: number;
  delayed: boolean;
  createdAt: string;
}

export interface Invoice {
  id: number;
  number: string;
  customerId: number;
  customerName?: string;
  jobId: number;
  total: number;
  paid: number;
  balance: number;
  dueDate?: string;
  status: 'DRAFT' | 'UNPAID' | 'PARTIAL' | 'PAID' | 'CANCELLED';
  overdue: boolean;
  cancelRequested: boolean;
  cancelReason?: string;
  createdAt: string;
}

export interface Payment {
  id: number;
  receiptNo: string;
  invoiceId: number;
  amount: number;
  method: 'CASH' | 'BANK_TRANSFER';
  reference: string;
  receivedBy: string;
  createdAt: string;
}

export interface StockItem {
  id: number;
  name: string;
  category: string;
  unit: string;
  qty: number;
  reorderLevel: number;
  low: boolean;
  createdAt: string;
}

export interface StockMovement {
  id: number;
  number: string;
  itemId: number;
  itemName?: string;
  type: 'RECEIVE' | 'ISSUE' | 'RETURN' | 'ADJUST';
  qty: number;
  jobId?: number;
  recipient: string;
  createdBy: string;
  reason: string;
  condition: string;
  createdAt: string;
}

export interface Supplier {
  id: number;
  name: string;
  phone: string;
  createdAt: string;
}

export interface Requisition {
  id: number;
  number: string;
  description: string;
  qty: number;
  estUnitPrice: number;
  itemId?: number;
  department: string;
  reason: string;
  requestedBy: string;
  status: 'DRAFT' | 'PENDING_APPROVAL' | 'APPROVED' | 'REJECTED' | 'ORDERED';
  createdAt: string;
}

export interface Lpo {
  id: number;
  number: string;
  requisitionId: number;
  supplierId: number;
  supplierName?: string;
  qty: number;
  unitPrice: number;
  total: number;
  status: 'PENDING_APPROVAL' | 'ORDERED' | 'RECEIVED' | 'REJECTED';
  grnNo?: string;
  createdAt: string;
}

export interface DashboardSummary {
  activeJobs: number;
  pendingQuotations: number;
  monthlyRevenue: number;
  outstandingInvoices: number;
  lowStockItems: number;
  pendingApprovals: number;
  todayJobs: Job[];
  recentQuotations: Quotation[];
}

export interface ReportSummary {
  totalRevenue: number;
  totalCollected: number;
  totalOutstanding: number;
  totalExpenses: number;
  totalJobs: number;
  completedJobs: number;
  cancelledJobs: number;
  monthlySales: { month: string; total: number }[];
}

export interface CompanyProfile {
  id: number;
  name: string;
  address: string;
  phone: string;
  crNumber: string;
  bankDetails: string;
  logoAttachmentId?: number;
}
