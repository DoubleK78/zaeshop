import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import ActivityLogResponse from "../../models/user/ActivityLogResponse";
import UserRoleSubscriptionResponse from "../../models/user/UserRoleSubscriptionResponse";
import ServerResponse from "../../models/common/ServerResponse";
import { PagingResponse } from "../../models/common/PagingResponse";
import PagingRequest from "../../models/common/PagingRequest";
import { AxiosRequestConfig } from "axios";
import axiosApiInstance from "../../services/interceptor";
import { identityServer, portalServer } from "../../services/baseUrls";
import { EActivityType } from "../../models/enums/EActivityType";
import UserRoleSubscriptionActivityPagingRequest from "../../models/user/UserRoleSubscriptionActivityPagingRequest";

// Thunks
const getActivitiesLogPagingAsyncThunk = createAsyncThunk<
    ServerResponse<PagingResponse<ActivityLogResponse>>,
    { id: string, params: PagingRequest, activityType: EActivityType },
    { rejectValue: string }
>('userRoleSubscription/getActivitiesLogPagingAsyncThunk', async (model, thunkApi) => {
    const config: AxiosRequestConfig<UserRoleSubscriptionActivityPagingRequest> = {
        params: {
            ...model.params,
            activityType: model.activityType
        }
    };
    const response = await axiosApiInstance.get<ServerResponse<PagingResponse<ActivityLogResponse>>>(portalServer + `/api/user/${model.id}/activity-log`, config);
    return response.data;
});

const getUserRoleSubscriptionAsyncThunk = createAsyncThunk<
    ServerResponse<UserRoleSubscriptionResponse>,
    { id: string },
    { rejectValue: string }
>('userRoleSubscription/getUserRoleSubscriptionAsyncThunk', async (model, thunkApi) => {
    const response = await axiosApiInstance.get<ServerResponse<UserRoleSubscriptionResponse>>(identityServer + `/api/user/${model.id}/role-subscription`);
    return response.data;
})

interface UserRoleSubscriptionState {
    roleSubscription: UserRoleSubscriptionResponse | null;
    activities: ActivityLogResponse[];
    totalRecords: number;
    loading: boolean;
    error: string | null;
}

const initialState: UserRoleSubscriptionState = {
    roleSubscription: null,
    activities: [],
    totalRecords: 0,
    loading: false,
    error: null
};

export const userRoleSubscriptionSlice = createSlice({
    name: " userRoleSubscription",
    initialState,
    reducers: {
    },
    extraReducers: (builder) => {
        // getActivitiesLogPagingAsyncThunk
        builder.addCase(getActivitiesLogPagingAsyncThunk.pending, (state) => {
            state.loading = true;
            state.error = null;
        });

        builder.addCase(getActivitiesLogPagingAsyncThunk.fulfilled, (state, action) => {
            state.loading = false;
            if (action.payload?.data) {
                state.activities = action.payload.data.data;
                state.totalRecords = action.payload.data.rowNum;
            }
        });

        builder.addCase(getActivitiesLogPagingAsyncThunk.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message ?? null;
        });

        // getUserRoleSubscriptionAsyncThunk
        builder.addCase(getUserRoleSubscriptionAsyncThunk.pending, (state) => {
            state.loading = true;
            state.error = null;
        });

        builder.addCase(getUserRoleSubscriptionAsyncThunk.fulfilled, (state, action) => {
            state.loading = false;
            if (action.payload?.data) {
                state.roleSubscription = action.payload.data;
            }
        });

        builder.addCase(getUserRoleSubscriptionAsyncThunk.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message ?? null;
        })
    },
});

export {
    getActivitiesLogPagingAsyncThunk,
    getUserRoleSubscriptionAsyncThunk
};
export default userRoleSubscriptionSlice.reducer;