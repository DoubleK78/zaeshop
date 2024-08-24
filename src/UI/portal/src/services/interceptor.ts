import axios, { AxiosResponse } from "axios";
import { loginFailure, loginSuccess, logout } from "../store/reducers/authSlice";
import { store } from "../store";
import { identityServer } from "./baseUrls";
import ServerResponse from "../models/common/ServerResponse";
import { toast } from "react-toastify";
import i18n from "../utils/i18n";

let refreshTokenAsyncFunc: Promise<AxiosResponse<any, any> | null> | undefined;
const refreshTokenAsync = async () => {
    try {
        const response = await axios.post(identityServer + "/api/account/refresh-token", {}, {
            withCredentials: true
        });
        dispatch(loginSuccess({ data: response.data, token: response.data.jwtToken }));
        return response;
    }
    catch (err: any) {
        dispatch(loginFailure(err.response.data));
        return null;
    }
}

const dispatch = store.dispatch;
const axiosApiInstance = axios.create({
    withCredentials: true
});

axiosApiInstance.interceptors.request.use((req) => {
    const token = localStorage.getItem("token");
    if (token) {
        req.headers.Authorization = `Bearer ${token}`;
    }
    return req;
}, error => {
    Promise.reject(error)
});

axiosApiInstance.interceptors.response.use(response => {
    return response;
}, async (error) => {
    const originalRequest = error.config;

    switch (error.response.status) {
        case 400: {
            const serverResponse = error.response.data as ServerResponse<any>;
            if (!serverResponse.isSuccess && serverResponse.errorMessage) {
                toast.dismiss();
                toast.error(i18n.t(serverResponse.errorMessage), {
                    hideProgressBar: true,
                    autoClose: 2000
                });
            }
            break;
        }
        case 401: {
            if (!originalRequest._retry) {
                originalRequest._retry = 0;
            }

            if (originalRequest._retry <= 5) {
                originalRequest._retry++;

                // Handling refresh token for multiple requests
                // First request (token is expried) will be handled by refreshTokenAsyncFunc
                // Other request will be used same of refreshTokenAsyncFunc by first request and all of them will be awaited util refreshTokenAsyncFunc complete
                // Retry All request with axiosApiInstance.request(originalRequest);
                if (!refreshTokenAsyncFunc) {
                    refreshTokenAsyncFunc = refreshTokenAsync();
                }
                const refreshTokenResult = await refreshTokenAsyncFunc;

                if (!refreshTokenResult) {
                    return error;
                }
                axiosApiInstance.defaults.headers.common['Authorization'] = 'Bearer ' + refreshTokenResult.data.jwtToken;
                return await axiosApiInstance.request(originalRequest);
            }
            else {
                dispatch(logout());
            }
            break;
        }
        case 403: {
            toast.dismiss();
            toast.error(i18n.t("toast.you_can_not_access_feature"), {
                hideProgressBar: true,
                autoClose: 2000
            });
            break;
        }
        case 500: {
            toast.dismiss();
            toast.error(i18n.t("toast.some_thing_is_wrong"));
            break;
        }
    }
    return error;
});

export default axiosApiInstance;