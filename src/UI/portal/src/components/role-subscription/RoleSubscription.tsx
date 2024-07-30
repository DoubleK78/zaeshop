import { useTranslation } from "react-i18next";
import { StoreState, useAppDispatch } from "../../store";
import { useSelector } from "react-redux";
import { useEffect, useMemo, useState } from "react";
import { getUserRoleSubscriptionAsyncThunk } from "../../store/reducers/userRoleSubscriptionSlice";
import dayjsCustom from "../../utils/dayjs/dayjs-custom";
import Select from "react-select";
import DropDownOption from "../../models/common/DropDownOption";
import RoleSubscriptionPopup from "./RoleSubscriptionPopup";
import ModalCommon from "../shared/ModalCommon";
import UserRoleSubscriptionHistoryAction from "../../models/user/UserRoleSubscriptionHistoryAction";
import React from "react";
import { updateRoleSubscription } from "../../services/role-subscription/roleSubscriptionService";
import { toast } from "react-toastify";
import UserRoleSubcriptionRequest from "../../models/user/UserRoleSubcriptionRequest";

type Props = {
    userId?: string | null;
    reloadPaging: () => void;
}

const rolesDropdown: DropDownOption<string>[] = [
    {
        label: 'User',
        value: 'User'
    },
    {
        label: 'User Premium',
        value: 'User Premium'
    },
    {
        label: 'User Super Premium',
        value: 'User Super Premium'
    }
]

const daysDropDown: DropDownOption<number | null>[] = [
    {
        label: 'None',
        value: null
    },
    {
        label: '30 days',
        value: 30
    },
    {
        label: '90 days',
        value: 90
    },
    {
        label: '365 days',
        value: 365
    }
];

const RoleSubscription: React.FC<Props> = ({ userId, reloadPaging }) => {
    const { t } = useTranslation();
    const dispatch = useAppDispatch();
    const { roleSubscription } = useSelector((state: StoreState) => state.userRoleSubscription);
    const [roleSelectedOption, setRoleSelectedOption] = useState<DropDownOption<string> | null>(null);
    const [daySelectedOption, setDaySelectedOption] = useState<DropDownOption<number | null> | null>({
        label: '30 days',
        value: 30
    });
    const [isOpen, setIsOpen] = useState(false);

    useEffect(() => {
        if (userId) {
            dispatch(getUserRoleSubscriptionAsyncThunk({ id: userId }));
        }
    }, [dispatch, userId]);

    useEffect(() => {
        setRoleSelectedOption(rolesDropdown.find(item => item.value === roleSubscription?.role) ?? null);
    }, [roleSubscription]);

    const onChangeRoleSelectedOption = (selectedOption: DropDownOption<string> | null) => {
        setRoleSelectedOption(selectedOption);
    }

    const onChangeDaySelectedOption = (selectedOption: DropDownOption<number | null> | null) => {
        setDaySelectedOption(selectedOption);
    }

    const onReset = () => {
        setRoleSelectedOption(rolesDropdown.find(item => item.value === roleSubscription?.role) ?? null);
        setDaySelectedOption({
            label: '30 days',
            value: 30
        })
    }

    const openModal = () => {
        setIsOpen(true);
    }

    const closeModal = (isReload?: boolean) => {
        if (isReload) {
            reloadPaging();
        }
        setIsOpen(false);
    }

    const onSubmit = async () => {
        closeModal();

        const toastId = toast.loading(t("toast.please_wait"), {
            hideProgressBar: true
        });

        const request: UserRoleSubcriptionRequest = {
            userId: userId!,
            role: roleSelectedOption!.value,
            days: roleSelectedOption!.value !== 'User' ? daySelectedOption?.value : null
        }

        const response: any = await updateRoleSubscription(userId!, request);
        if (response.status !== 200) {
            toast.update(toastId, {
                render: t(response.response.data), type: toast.TYPE.ERROR, isLoading: false,
                autoClose: 2000
            });
            return;
        }

        toast.update(toastId, {
            render: t("toast.create_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
            autoClose: 2000
        });

        // Referesh
        if (userId) {
            dispatch(getUserRoleSubscriptionAsyncThunk({ id: userId }));
        }

        reloadPaging();
    }

    const activyHistory = useMemo(() => {
        const history: UserRoleSubscriptionHistoryAction = {
            role: roleSelectedOption?.label,
            day: roleSelectedOption?.value !== 'User' ? daySelectedOption?.value : null
        };
        return history;
    }, [roleSelectedOption, daySelectedOption]);

    return (
        <>
            {/* end page title end breadcrumb */}
            <div className="row">
                <div className="col-12">
                    <div className="card">
                        <div className="card-body">
                            <div className="met-profile">
                                <div className="row">
                                    <div className="col-lg-4 align-self-center mb-3 mb-lg-0">
                                        <div className="met-profile-main">
                                            <div className="met-profile-main-pic">
                                                <img
                                                    src={roleSubscription?.avatar ?? ''}
                                                    alt=""
                                                    height={110}
                                                    className="rounded-circle"
                                                />
                                            </div>
                                            <div className="met-profile_user-detail">
                                                <h3 className="met-user-name">{roleSubscription?.fullName}</h3>
                                                <p className="mb-0 met-user-name-post">
                                                    {roleSubscription?.role}
                                                </p>
                                                <p className="mb-0 met-user-name-post">
                                                    {t('role_subscription.expried_date')}: {
                                                        roleSubscription?.expriedRoleDate ?
                                                            dayjsCustom.utc(roleSubscription?.expriedRoleDate).local().format('DD-MM-YYYY HH:mm') :
                                                            (roleSubscription?.role === 'User' ? t('role_subscription.none') : t('role_subscription.forever'))
                                                    }
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                    {/*end col*/}
                                    <div className="col-lg-4 ms-auto align-self-center">
                                        <ul className="list-unstyled personal-detail mb-0">
                                            <li className="">
                                                <i className="fa-regular fa-address-card text-secondary" />{"  "}
                                                <b>{t('role_subscription.username')} </b> : {roleSubscription?.username}
                                            </li>
                                            <li className="mt-2">
                                                <i className="fa-regular fa-envelope text-secondary" />{" "}
                                                <b>{t('role_subscription.email')} </b> : {roleSubscription?.email}
                                            </li>
                                            <li className="mt-2">
                                                <i className="fa-regular fa-calendar text-secondary" />{" "}
                                                <b>{t('role_subscription.created_on')} </b> : {dayjsCustom.utc(roleSubscription?.createdOnUtc).local().format('DD-MM-YYYY HH:mm')}
                                            </li>
                                        </ul>
                                    </div>
                                    {/*end col*/}
                                    <div className="col-lg-4 align-self-center">
                                    </div>
                                    {/*end col*/}
                                </div>
                                {/*end row*/}
                            </div>
                            {/*end f_profile*/}
                        </div>
                        {/*end card-body*/}
                    </div>
                    {/*end card*/}
                </div>
                {/*end col*/}
            </div>
            {/*end row*/}
            <div className="row">
                <div className="card">
                    <div className="card-title">
                        <div className="card-header">
                            <div className="row align-items-center">
                                <div className="col">
                                    <h4 className="card-title">{t('role_subscription.update_subscription')}</h4>
                                </div>
                                {/*end col*/}
                            </div>{" "}
                            {/*end row*/}
                        </div>
                    </div>
                    <div className="card-body">
                        <div className="row">
                            <div className="col-lg-6">
                                <div className="mb-3 row">
                                    <label className="col-sm-2 text-muted text-end align-middle mt-2">
                                        {t('role_subscription.role')}
                                    </label>
                                    <div className="col-sm-10">
                                        <Select
                                            className="basic-single"
                                            classNamePrefix="select"
                                            options={rolesDropdown}
                                            value={roleSelectedOption}
                                            onChange={onChangeRoleSelectedOption}
                                            isSearchable={true}
                                            isClearable={true}
                                        />
                                    </div>
                                </div>
                            </div>
                            {roleSelectedOption?.value !== 'User' && (
                                <div className="col-lg-6">
                                    <div className="mb-3 row">
                                        <label className="col-sm-2 text-muted text-end align-middle mt-2">
                                            {t('role_subscription.day')}
                                        </label>
                                        <div className="col-sm-10">
                                            <Select
                                                className="basic-single"
                                                classNamePrefix="select"
                                                options={daysDropDown}
                                                value={daySelectedOption}
                                                onChange={onChangeDaySelectedOption}
                                                isSearchable={true}
                                                isClearable={true}
                                            />
                                        </div>
                                    </div>
                                </div>
                            )}
                        </div>
                        <button
                            type="button"
                            className="btn btn-light"
                            onClick={onReset}
                        >
                            {t('role_subscription.reset')}
                        </button>
                        <button
                            type="button"
                            className="btn btn-primary"
                            onClick={openModal}
                        >
                            {t('role_subscription.submit')}
                        </button>
                    </div>
                </div>
            </div >
            <ModalCommon
                props={{ modalIsOpen: isOpen, openModal, closeModal, onSubmit, roleSubscription, activyHistory }}
                Component={RoleSubscriptionPopup}
            />
        </>
    );
}

export default React.memo(RoleSubscription);