import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import User from '../../models/user/User';
import { PagingResponse } from '../../models/common/PagingResponse';

interface UserState {
  totalRecords: number;
  users: User[];
  loading: boolean;
  error: string | null;
  isTrigger: boolean; // CUD operation and trigger fetch users
}

const initialState: UserState = {
  totalRecords: 0,
  users: [],
  loading: false,
  error: null,
  isTrigger: true
};

const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {
    // Add user reducer
    userAdded(state, action: PayloadAction<User>) {
      state.users.push(action.payload);
      state.isTrigger = true;
    },

    // Update user reducer
    userUpdated(state, action: PayloadAction<{ id: string; updatedUser: Partial<User> }>) {
      const { id, ...updatedUser } = action.payload;
      const index = state.users.findIndex((user) => user.id === id);
      if (index !== -1) {
        state.users[index] = { ...state.users[index], ...updatedUser };
      }

      state.isTrigger = true;
    },

    // Delete user reducer
    userDeleted(state, action: PayloadAction<string>) {
      const id = action.payload;
      state.users = state.users.filter((user) => user.id !== id);

      state.isTrigger = true;
    },

    // Fetch users reducers
    fetchUsersStart(state) {
      state.loading = true;
      state.error = null;
      state.isTrigger = true;
    },
    fetchUsersSuccess(state, action: PayloadAction<PagingResponse<User>>) {
      state.loading = false;
      state.error = null;
      state.users = action.payload.data;
      state.totalRecords = action.payload.rowNum;
      state.isTrigger = false;
    },
    fetchUsersFailure(state, action: PayloadAction<string>) {
      state.loading = false;
      state.error = action.payload;
      state.users = [];
      state.totalRecords = 0;
      state.isTrigger = false;
    },
  },
});

export const {
  userAdded,
  userUpdated,
  userDeleted,
  fetchUsersStart,
  fetchUsersSuccess,
  fetchUsersFailure,
} = userSlice.actions;

export default userSlice.reducer;