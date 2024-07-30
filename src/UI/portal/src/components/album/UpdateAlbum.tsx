import './UpdateAlbum.css';
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
import { FilePond } from "react-filepond";
import { ActualFileObject, FilePondFile } from "filepond";
import { ERegion } from '../../models/enums/Eregion';

type UpdateAlbumProps = {
    album: AlbumPagingResponse,
    closeModal: (isReload?: boolean) => void;
}

const UpdateAlbum: React.FC<UpdateAlbumProps> = ({ album, closeModal }) => {
    const [t] = useTranslation();
    const [thumbnailFiles, setThumbnailFiles] = useState<(ActualFileObject)[]>([]);
    const [backgroundFiles, setBackgroundFiles] = useState<(ActualFileObject)[]>([]);

    const [isUpdateThumbnail, setIsUpdateThumbnail] = useState<boolean>(false);
    const [isUpdateOriginalUrl, setIsUpdateOriginalUrl] = useState<boolean>(false);

    const { albumAlertMessages, contentTypes } = useSelector((state: StoreState) => state.album);
    const [region, setRegion] = useState<string>(album?.region === ERegion.en ? 'en' : 'vi');

    const onUpdateFiles = (filesPondFiles: FilePondFile[]) => {
        const files = filesPondFiles.map(item => item.file);
        setThumbnailFiles(files);
        setIsUpdateThumbnail(true);
    }

    const onUpdateBackgroundFiles = (filesPondFiles: FilePondFile[]) => {
        const files = filesPondFiles.map(item => item.file);
        setBackgroundFiles(files);
        setIsUpdateOriginalUrl(true);
    }

    const onChangeImageThumbnail = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
        setIsUpdateThumbnail(true);
        event.preventDefault();
    }

    const onChangeImageBackground = (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
        setIsUpdateOriginalUrl(true);
        event.preventDefault();
    }

    // Dropdown options
    const contentTypesDropDown = useMemo((): DropDownOption<number>[] => {
        if (!contentTypes) return [];

        const regionCode = region === 'en' ? ERegion.en : ERegion.vi;
        return contentTypes.filter(o => o.region === regionCode).map((contentType: ContentType): DropDownOption<number> => ({
            value: contentType.id,
            label: contentType.name ?? ''
        }));
    }, [contentTypes, region]);

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

        const request: AlbumRequestModel = {
            ...albumRequestModel,
            albumAlertMessageId: albumAlertMessageSelectedOption ? Number(albumAlertMessageSelectedOption.value) : undefined,
            contentTypeIds: contentTypesSelectedOptions?.map(option => Number(option.value)),
            isUpdateThumbnail,
            isUpdateOriginalUrl,
            region
        };

        if (thumbnailFiles.length > 0) {
            request.fileNameThumbnail = thumbnailFiles[0].name;
        }

        if (backgroundFiles.length > 0) {
            request.fileNameBackground = backgroundFiles[0].name;
        }

        const response = await updateAlbum(album.id, request);

        if (response.status === 200) {
            toast.update(toastId, {
                render: t("toast.update_sucessfully"),
                isLoading: false,
                type: toast.TYPE.SUCCESS,
                autoClose: 2000
            });

            closeModal(true);
            return;
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
                                    <div className="mb-3 row">
                                        <label className="col-sm-2 col-form-label text-end">{t('album.filter_region')}</label>
                                        <div className="col-sm-10">
                                            <select className="form-select"
                                                style={{ width: "auto" }}
                                                value={region}
                                                onChange={(event: React.ChangeEvent<HTMLSelectElement>) => setRegion((event.target.value))}>
                                                <option value={'vi'}>{t('album.filter_region_vietnam')}</option>
                                                <option value={'en'}>{t('album.filter_region_english')}</option>
                                            </select>
                                        </div>
                                    </div>

                                    <div className="mb-3 row text-center">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album.modal.thumbnail')}
                                        </label>
                                        {album?.cdnThumbnailUrl && !isUpdateThumbnail &&
                                            <div>
                                                <img src={album?.cdnThumbnailUrl} alt={t('album.modal.thumbnail')}
                                                    style={{
                                                        maxWidth: "100%",
                                                        height: "auto"
                                                    }} />
                                                <button type='button' onClick={(event) => onChangeImageThumbnail(event)} className='btn'>
                                                    <i className="fa-solid fa-circle-xmark text-danger font-16 icon-remove-image"
                                                    ></i>
                                                </button>
                                            </div>
                                        }
                                        {(!album?.cdnThumbnailUrl || isUpdateThumbnail) && <FilePond
                                            files={thumbnailFiles}
                                            onupdatefiles={onUpdateFiles}
                                            maxFiles={1}
                                            name="files"
                                            labelIdle='Drag & Drop your file or <span class="filepond--label-action">Browse</span>'
                                            beforeAddFile={(file) => {
                                                return new Promise((resolve) => {
                                                    if (!file.fileType.includes('image/')) {
                                                        resolve(false);
                                                    }
                                                    resolve(true);
                                                })
                                            }}
                                        />
                                        }
                                    </div>

                                    <div className="mb-3 row text-center">
                                        <label
                                            className="col-sm-2 col-form-label text-end">
                                            {t('album.modal.background')}
                                        </label>
                                        {album?.cdnOriginalUrl && !isUpdateOriginalUrl &&
                                            <div>
                                                <img src={album?.cdnOriginalUrl} alt={t('album.modal.thumbnail')}
                                                    style={{
                                                        maxWidth: "100%",
                                                        height: "auto"
                                                    }} />
                                                <button type='button' onClick={(event) => onChangeImageBackground(event)} className='btn'>
                                                    <i className="fa-solid fa-circle-xmark text-danger font-16 icon-remove-image"
                                                    ></i>
                                                </button>
                                            </div>
                                        }
                                        {(!album?.cdnOriginalUrl || isUpdateOriginalUrl) && <FilePond
                                            files={backgroundFiles}
                                            onupdatefiles={onUpdateBackgroundFiles}
                                            maxFiles={1}
                                            name="files"
                                            labelIdle='Drag & Drop your file or <span class="filepond--label-action">Browse</span>'
                                            beforeAddFile={(file) => {
                                                return new Promise((resolve) => {
                                                    if (!file.fileType.includes('image/')) {
                                                        resolve(false);
                                                    }
                                                    resolve(true);
                                                })
                                            }}
                                        />
                                        }
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
