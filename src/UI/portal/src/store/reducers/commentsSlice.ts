import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import CommentManagementResponse from "../../models/comments/CommentManagementResponse";
import ServerResponse from "../../models/common/ServerResponse";
import axiosApiInstance from "../../services/interceptor";
import { portalServer } from "../../services/baseUrls";
import { PagingResponse } from "../../models/common/PagingResponse";
import { CommentManagementRequestModel } from "../../models/comments/CommentManagementRequest";

// Thunks
const getCommentsAsyncThunk = createAsyncThunk<
    ServerResponse<PagingResponse<CommentManagementResponse>>,
    { params: CommentManagementRequestModel },
    { rejectValue: string }
>('commentManagement/getCommentsAsyncThunk', async (model, thunkApi) => {
    const response = await axiosApiInstance.get<ServerResponse<PagingResponse<CommentManagementResponse>>>(portalServer + '/api/user/comments/check', {
        params: model.params
    });
    return response.data;
});

interface CommentManagementState {
    comments: CommentManagementResponse[] | null;
    totalRecords: number;
    loading: boolean;
    error: string | null;
}

const initialState: CommentManagementState = {
    comments: null,
    totalRecords: 0,
    loading: false,
    error: null
};


export const commentManagementSlice = createSlice({
    name: 'commentManagement',
    initialState,
    reducers: {
    },
    extraReducers: (builder) => {
        // getCommentsAsyncThunk
        builder.addCase(getCommentsAsyncThunk.pending, (state) => {
            state.loading = true;
            state.error = null;
        });

        builder.addCase(getCommentsAsyncThunk.fulfilled, (state, action) => {
            state.loading = false;
            if (action.payload?.data) {
                state.comments = action.payload.data.data;
                state.totalRecords = action.payload.data.rowNum;
            }
        });

        builder.addCase(getCommentsAsyncThunk.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message ?? null;
        });
    },
})

export { getCommentsAsyncThunk }
export default commentManagementSlice.reducer;