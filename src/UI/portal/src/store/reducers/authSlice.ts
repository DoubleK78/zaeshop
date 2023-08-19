import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import User from '../../models/User';

interface AuthState {
    token: string | null;
    user: User | null;
    isAuthenticate: boolean;
    loading: boolean;
    error: string | null;
}

const initialState: AuthState = {
    token: localStorage.getItem("token"),
    user: {
        ...JSON.parse(localStorage.getItem("user") || '{}')
    },
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

            state.loading = false;
            state.token = action.payload.token;
            state.user = {
                ...action.payload.data
            };
            state.isAuthenticate = true;
        },
        loginFailure(state, action: PayloadAction<string>) {
            localStorage.removeItem('token');
            localStorage.removeItem("user");
            state.loading = false;
            state.error = action.payload;
            state.isAuthenticate = false;
        },
        logout(state) {
            localStorage.removeItem('token');
            localStorage.removeItem("user");
            state.token = '';
            state.error = null;
            state.isAuthenticate = false;
        },
    },
});

export const { loginStart, loginSuccess, loginFailure, logout } = authSlice.actions;

export default authSlice.reducer;
