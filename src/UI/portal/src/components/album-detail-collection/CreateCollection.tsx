import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import CollectionRequestModel from "../../models/album-detail-collection/CollectionRequestModel";
import { toast } from "react-toastify";
import { createCollection } from "../../services/album-detail-collection/albumDetailCollectionService";
import classNames from "classnames";

type CreateCollectionProps = {
    closeModal: (isReload?: boolean) => void;
    albumId: number;
};

const CreateCollection: React.FC<CreateCollectionProps> = ({ closeModal, albumId }: CreateCollectionProps) => {
    const [t] = useTranslation();
    const [isPriority, setIsPriority] = useState(false);

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<CollectionRequestModel>({
        defaultValues: {
            albumId,
            title: '',
            description: '',
            extendName: '',
        }
    });

    const onSubmit = async (collectionRequestModel: CollectionRequestModel) => {
        const toastId = toast.loading(t("toast.please_wait"), {
            hideProgressBar: true
        });
        await createCollection({
            ...collectionRequestModel,
            isPriority
        });

        toast.update(toastId, {
            render: t("toast.create_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
            autoClose: 2000
        });
        closeModal(true);
    };

    return (
        <>
            <div
                className="modal fade show d-block"
                id="exampleModalLogin"
                tabIndex={-1}
                role="dialog"
                aria-labelledby="exampleModalDefaultLogin"
                aria-hidden="true">
                <div className="modal-dialog" role="document">
                    <form onSubmit={handleSubmit((data) => onSubmit(data))}>
                        <div className="modal-content">
                            <div className="modal-header">
                                <h6 className="modal-title m-0" id="exampleModalDefaultLogin">
                                    {t('album_detail.modal.create_collection')}
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
                                    <div className="form-check form-switch form-switch-success mb-4">
                                        <label className="form-check-label" htmlFor="customSwitchPrimary">
                                            {t('album_detail.spre_priority')}
                                        </label>
                                        <input
                                            className="form-check-input"
                                            type="checkbox"
                                            id="customSwitchPrimary"
                                            checked={isPriority}
                                            onChange={() => setIsPriority(!isPriority)}
                                        />
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album_detail.title')}
                                        </label>
                                        <div className="col-sm-10">
                                            <input
                                                className="form-control"
                                                type="text"
                                                {...register('title', { required: true })}
                                            />
                                            <div className={classNames("invalid-feedback", {
                                                "d-inline": errors.title
                                            })}>
                                                <p>{t('album_detail.title_is_required')}</p>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album_detail.description')}
                                        </label>
                                        <div className="col-sm-10">
                                            <input
                                                className="form-control"
                                                type="text"
                                                {...register('description')}
                                            />
                                        </div>
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album_detail.extend_name')}
                                        </label>
                                        <div className="col-sm-10">
                                            <input
                                                className="form-control"
                                                type="text"
                                                {...register('extendName')}
                                            />
                                        </div>
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album_detail.volume')}
                                        </label>
                                        <div className="col-sm-10">
                                            <input
                                                className="form-control"
                                                type="number"
                                                {...register('volume', { min: 0, valueAsNumber: true })}
                                            />
                                            <div className={classNames("invalid-feedback", {
                                                "d-inline": errors.volume
                                            })}>
                                                <p>{t('album_detail.volume_is_invalid')}</p>
                                            </div>
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
}

export default React.memo(CreateCollection);