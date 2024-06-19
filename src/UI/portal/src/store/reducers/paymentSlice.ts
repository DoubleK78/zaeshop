import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { PaymentManagementResponse } from "../../models/payments/PaymentManagementResponse";
import ServerResponse from "../../models/common/ServerResponse";
import { PagingResponse } from "../../models/common/PagingResponse";
import PagingRequest from "../../models/common/PagingRequest";
import axiosApiInstance from "../../services/interceptor";
import { portalServer } from "../../services/baseUrls";

// Thunks
const getPaymentsAsyncThunk = createAsyncThunk<
    ServerResponse<PagingResponse<PaymentManagementResponse>>,
    { params: PagingRequest },
    { rejectValue: string }
>('paymentManagement/getPaymentsAsyncThunk', async (model, thunkApi) => {
    const response = await axiosApiInstance.get<ServerResponse<PagingResponse<PaymentManagementResponse>>>(portalServer + '/api/user/activity-logs', {
        params: model.params
    });
    return response.data;
});

interface PaymentManagementState {
    payments: PaymentManagementResponse[] | null;
    totalRecords: number;
    loading: boolean;
    error: string | null;
}

const initialState: PaymentManagementState = {
    payments: null,
    totalRecords: 0,
    loading: false,
    error: null
};

export const paymentManagementSlice = createSlice({
    name: 'paymentManagement',
    initialState,
    reducers: {
    },
    extraReducers: (builder) => {
        // getPaymentsAsyncThunk
        builder.addCase(getPaymentsAsyncThunk.pending, (state) => {
            state.loading = true;
            state.error = null;
        });

        builder.addCase(getPaymentsAsyncThunk.fulfilled, (state, action) => {
            state.loading = false;
            if (action.payload?.data) {
                state.payments = action.payload.data.data;
                state.totalRecords = action.payload.data.rowNum;
            }
        });

        builder.addCase(getPaymentsAsyncThunk.rejected, (state, action) => {
            state.loading = false;
            state.error = action.error.message ?? null;
        });
    },
});

export { getPaymentsAsyncThunk }
export default paymentManagementSlice.reducer;