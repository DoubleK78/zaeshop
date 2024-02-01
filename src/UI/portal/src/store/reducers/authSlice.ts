import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import User from '../../models/user/User';

interface AuthState {
    token: string | null;
    user: User | null;
    roles: Array<string> | null;
    isAuthenticate: boolean;
    loading: boolean;
    error: string | null;
}

const initialState: AuthState = {
    token: localStorage.getItem("token"),
    user: {
        ...JSON.parse(localStorage.getItem("user") ?? '{}')
    },
    roles: JSON.parse(localStorage.getItem("roles") ?? '[]'),
    isAuthenticate: !!localStorage.getItem("token"),
    loading: false,
    error: null,
};

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        loginStart(state) {
            state.loading = true;
            state.error = null;
        },
        loginSuccess(state, action: PayloadAction<{ data: any, token: string }>) {
            localStorage.setItem("token", action.payload.token);
            localStorage.setItem("user", JSON.stringify(action.payload.data));
            localStorage.setItem("roles", JSON.stringify(action.payload.data?.roles));

            state.loading = false;
            state.token = action.payload.token;
            state.user = {
                ...action.payload.data
            };
            state.roles = action.payload.data?.roles;
            state.isAuthenticate = true;
        },
        loginFailure(state, action: PayloadAction<string>) {
            localStorage.removeItem('token');
            localStorage.removeItem("user");
            localStorage.removeItem("roles");
            state.loading = false;
            state.error = action.payload;
            state.isAuthenticate = false;
            state.user = null;
            state.roles = null;
        },
        logout(state) {
            localStorage.removeItem('token');
            localStorage.removeItem("user");
            localStorage.removeItem("roles");
            state.token = '';
            state.error = null;
            state.isAuthenticate = false;
            state.user = null;
            state.roles = null;
        },
    },
});

export const { loginStart, loginSuccess, loginFailure, logout } = authSlice.actions;

export default authSlice.reducer;
