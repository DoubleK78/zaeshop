import React, { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useSelector } from 'react-redux';
import { toast } from 'react-toastify';
import { useTranslation } from 'react-i18next';
import { updateAlbum } from '../../services/album/albumService';
import { StoreState } from '../../store';
import DropDownOption from '../../models/common/DropDownOption';
import ContentType from '../../models/content-type/ContentType';
import AlbumRequestModel from '../../models/album/AlbumRequestModel';
import AlbumPagingResponse from '../../models/album/AlbumPagingResponse';
import Select from 'react-select';
import classNames from 'classnames';
import AlbumAlertMessage from '../../models/album-alert-mesage/AlbumAlertMessage';

type UpdateAlbumProps = {
    album: AlbumPagingResponse,
    closeModal: (isReload?: boolean) => void;
}

const UpdateAlbum: React.FC<UpdateAlbumProps> = ({ album, closeModal }) => {
    const [t] = useTranslation();
    const { albumAlertMessages, contentTypes } = useSelector((state: StoreState) => state.album);

    // Dropdown options
    const contentTypesDropDown = useMemo((): DropDownOption<number>[] => {
        if (!contentTypes) return [];
        return contentTypes.map((contentType: ContentType): DropDownOption<number> => ({
            value: contentType.id,
            label: contentType.name ?? ''
        }));
    }, [contentTypes]);

    const albumAlertMessageDropDown = useMemo((): DropDownOption<number>[] => {
        if (!albumAlertMessages) return [];
        return albumAlertMessages.map((albumAlertMessage: AlbumAlertMessage): DropDownOption<number> => ({
            value: albumAlertMessage.id,
            label: albumAlertMessage.name ?? ''
        }));
    }, [albumAlertMessages]);

    const contentTypeIds = useMemo((): number[] => {
        const listContentTypeIds = album?.contentTypeIds?.split(',').map(item => Number(item)) ?? [];
        return listContentTypeIds;
    }, [album.contentTypeIds])

    // Use to build model
    const [albumAlertMessageSelectedOption, setAlbumAlertMessageSelectedOption] = useState<DropDownOption<number> | null>(
        albumAlertMessageDropDown.find(item => Number(item.value) === album.albumAlertMessageId) ?? null
    );
    const [contentTypesSelectedOptions, setContentTypesSelectedOptions] = useState<DropDownOption<number>[] | null>(
        contentTypesDropDown.filter(item => contentTypeIds.includes(item.value))
    );

    const onChangeAlbumAlertMessageSelectedOption = (selectedOption: DropDownOption<number> | null) => {
        setAlbumAlertMessageSelectedOption(selectedOption);
    }

    const onChangeContentTypesSelectedOptions = (selectedOptions: any) => {
        setContentTypesSelectedOptions(selectedOptions);
    }

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<AlbumRequestModel>({
        defaultValues: {
            title: album.title,
            description: album.description
        }
    });

    const onSubmit = async (albumRequestModel: AlbumRequestModel) => {
        const toastId = toast.loading(t("toast.please_wait"), {
            hideProgressBar: true
        });
        const response = await updateAlbum(album.id, {
            ...albumRequestModel,
            albumAlertMessageId: albumAlertMessageSelectedOption ? Number(albumAlertMessageSelectedOption.value) : undefined,
            contentTypeIds: contentTypesSelectedOptions?.map(option => Number(option.value))
        });
        if (response.status === 200) {
            toast.update(toastId, {
                render: t("toast.update_sucessfully"),
                type: toast.TYPE.SUCCESS,
                autoClose: 2000
            });

            closeModal(true);
        }
        toast.done(toastId);
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
                                    {t('album.modal.update_album')}
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
                                    <div className="mb-3 row">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album.modal.title')}
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
                                                <p>{t('album.modal.title_is_required')}</p>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album.modal.description')}
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
                                            {t('album.modal.alert_message')}
                                        </label>
                                        <div className="col-sm-10">
                                            <Select
                                                className="basic-single"
                                                classNamePrefix="select"
                                                options={albumAlertMessageDropDown}
                                                value={albumAlertMessageSelectedOption}
                                                onChange={onChangeAlbumAlertMessageSelectedOption}
                                                isSearchable={true}
                                                isClearable={true}
                                            />
                                        </div>
                                    </div>
                                    <div className="mb-3 row">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album.modal.content_types')}
                                        </label>
                                        <div className="col-sm-10">
                                            <Select
                                                className="basic-multi-select"
                                                classNamePrefix="select"
                                                options={contentTypesDropDown}
                                                value={contentTypesSelectedOptions}
                                                onChange={onChangeContentTypesSelectedOptions}
                                                isMulti
                                                isSearchable={true}
                                                isClearable={true}
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
}

export default UpdateAlbum;
