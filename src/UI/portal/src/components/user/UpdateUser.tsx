import React, { useEffect, useMemo, useState } from 'react';
import User from '../../models/user/User';
import { useDispatch, useSelector } from 'react-redux';
import { updateUser } from '../../store/thunks/userThunk';
import UserUpdateRequestModel from '../../models/user/UserUpdateRequestModel';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import classNames from 'classnames';
import Select from 'react-select';
import DropDownOption from '../../models/common/DropDownOption';
import { StoreState } from '../../store';
import Role from '../../models/role/Role';
import { getAllRoles } from '../../store/thunks/roleThunk';
import { toast } from 'react-toastify';

type UpdateUserProps = {
    user: User;
    closeModal: (isReload?: boolean) => void;
};

const UpdateUser: React.FC<UpdateUserProps> = ({ user, closeModal }) => {
    const dispatch = useDispatch();
    const [t] = useTranslation();
    const [selectedOptions, setSelectedOptions] = useState<DropDownOption<string>[]>([]);
    const [isBanned, setIsBanned] = useState<boolean>(user.isBanned ?? false);

    const roles = useSelector((state: StoreState) => state.role.roles);
    const rolesDropDown = useMemo((): DropDownOption<string>[] => {
        if (!roles) return [];
        return roles.map((role: Role): DropDownOption<string> => ({
            value: role.name ?? '',
            label: role.name ?? ''
        }));
    }, [roles]);

    useEffect(() => {
        getAllRoles()(dispatch);
    }, [dispatch]);

    useEffect(() => {
        setSelectedOptions(rolesDropDown.filter(r => user.roles?.split(',').includes(r.value)));
    }, [user.roles, rolesDropDown])

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<UserUpdateRequestModel>({
        defaultValues: {
            fullName: user.fullName ?? '',
            password: '',
        }
    });

    const onSubmit = async (userUpdateRequestModel: UserUpdateRequestModel) => {
        const toastId = toast.loading(t("toast.please_wait"), {
            hideProgressBar: true
        });
        await updateUser(user.id, {
            ...userUpdateRequestModel,
            roles: selectedOptions?.map(option => option.value),
            isBanned
        })(dispatch);

        toast.update(toastId, {
            render: t("toast.update_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
            autoClose: 2000
        });
        closeModal(true);
    };

    const onHandleChange = (selectedOption: any) => {
        setSelectedOptions(selectedOption);
    };

    return (
        <>
            <div
                className="modal fade show d-block"
                id="exampleModalLogin"
                tabIndex={-1}
                role="dialog"
                aria-labelledby="exampleModalDefaultLogin"
                aria-hidden="true"
            >
                <div className="modal-dialog" role="document">
                    <form onSubmit={handleSubmit((data) => onSubmit(data))}>
                        <div className="modal-content">
                            <div className="modal-header">
                                <h6 className="modal-title m-0" id="exampleModalDefaultLogin">
                                    {t('user.modal.update_user')}
                                </h6>
                                <button
                                    type="button"
                                    className="btn-close"
                                    data-bs-dismiss="modal"
                                    aria-label="Close"
                                    onClick={() => closeModal()}
                                />
                            </div>
                            {/*end modal-header*/}
                            <div className="modal-body">
                                <div className="card-body">
                                    <div className="form-check form-switch form-switch-danger mb-4">
                                        <label className="form-check-label" htmlFor="customSwitchPrimary">
                                            {t('user.banned_title')}
                                        </label>
                                        <input
                                            className="form-check-input"
                                            type="checkbox"
                                            id="customSwitchPrimary"
                                            checked={isBanned}
                                            onChange={() => setIsBanned(!isBanned)}
                                        />
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            htmlFor="example-text-input"
                                            className="col-sm-2 col-form-label text-end">
                                            {t('user.full_name')}
                                        </label>
                                        <div className="col-sm-10">
                                            <input
                                                className="form-control"
                                                type="text"
                                                defaultValue="Artisanal kale"
                                                id="example-text-input"
                                                {...register('fullName', { required: true })}
                                            />
                                            <div className={classNames("invalid-feedback", {
                                                "d-inline": errors.fullName
                                            })}>
                                                <p>{t('user.full_name_is_required')}</p>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            htmlFor="example-text-input"
                                            className="col-sm-2 col-form-label text-end">
                                            {t('user.password')}
                                        </label>
                                        <div className="col-sm-10">
                                            <input
                                                className="form-control"
                                                type="text"
                                                defaultValue="Artisanal kale"
                                                id="example-text-input"
                                                {...register('password')}
                                            />
                                        </div>
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            htmlFor="example-text-input"
                                            className="col-sm-2 col-form-label text-end">
                                            {t('user.role')}
                                        </label>
                                        <div className="col-sm-10">
                                            <Select
                                                className="basic-multi-select"
                                                classNamePrefix="select"
                                                options={rolesDropDown}
                                                value={selectedOptions}
                                                onChange={onHandleChange}
                                                isMulti
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                            {/*end modal-body*/}
                            <div className="modal-footer">
                                <button
                                    type="button"
                                    className="btn btn-de-secondary btn-sm"
                                    data-bs-dismiss="modal"
                                    onClick={() => closeModal()}
                                >
                                    {t('user.modal.close')}
                                </button>
                                <button type="submit" className="btn btn-primary btn-sm">
                                    {t('user.modal.save_changes')}
                                </button>
                            </div>
                            {/*end modal-footer*/}
                        </div>
                        {/*end modal-content*/}
                    </form>
                </div>
                {/*end modal-dialog*/}
            </div>
            {/*end modal*/}
        </>
    );
};

export default UpdateUser;