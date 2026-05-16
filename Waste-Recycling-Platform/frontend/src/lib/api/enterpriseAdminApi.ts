import { API_CONFIG } from '@/lib/api/config';

export interface EnterpriseListItem {
  id: string;
  companyName: string;
  status: 'Pending' | 'Verified' | 'Rejected';
  serviceArea?: string;
  createdAt: string;
}

export const enterpriseAdminApi = {
  getEnterprises: async (page: number, pageSize: number, isVerified?: boolean, search?: string) => {
    let url = `${API_CONFIG.BASE_URL}/admin/enterprises?page=${page}&pageSize=${pageSize}`;
    if (isVerified !== undefined) url += `&isVerified=${isVerified}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    
    const token = localStorage.getItem('token');
    const res = await fetch(url, { headers: { 'Authorization': `Bearer ${token}` } });
    if (!res.ok) throw new Error('Failed to fetch enterprises');
    return res.json();
  },
  
  getEnterpriseDetail: async (id: string) => {
    const token = localStorage.getItem('token');
    const res = await fetch(`${API_CONFIG.BASE_URL}/admin/enterprises/${id}`, { headers: { 'Authorization': `Bearer ${token}` } });
    if (!res.ok) throw new Error('Failed to fetch enterprise detail');
    return res.json();
  },

  verifyEnterprise: async (id: string) => {
    const token = localStorage.getItem('token');
    const res = await fetch(`${API_CONFIG.BASE_URL}/admin/enterprises/${id}/verify`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error('Failed to verify enterprise');
    return res.json();
  },

  rejectEnterprise: async (id: string, reason: string) => {
    const token = localStorage.getItem('token');
    const res = await fetch(`${API_CONFIG.BASE_URL}/admin/enterprises/${id}/reject`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason })
    });
    if (!res.ok) throw new Error('Failed to reject enterprise');
    return res.json();
  }
};