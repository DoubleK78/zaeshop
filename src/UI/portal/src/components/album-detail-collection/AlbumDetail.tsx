import React, { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { StoreState, useAppDispatch } from "../../store";
import { useSelector } from "react-redux";
import DropDownOption from "../../models/common/DropDownOption";
import ContentType from "../../models/content-type/ContentType";
import AlbumAlertMessage from "../../models/album-alert-mesage/AlbumAlertMessage";
import { getAlbumAlertMessagesAsyncThunk, getAllContentTypesAsyncThunk } from "../../store/reducers/albumSlice";
import { getAlbumDetailAsyncThunk } from "../../store/reducers/albumDetailCollectionSlice";
import { useForm } from "react-hook-form";
import { toast } from "react-toastify";
import { clearCacheComicPage, updateAlbumDetail } from "../../services/album-detail-collection/albumDetailCollectionService";
import AlbumDetailRequest from "../../models/album-detail-collection/AlbumDetailRequest";
import Select from "react-select";
import classNames from "classnames";
import { ERegion } from "../../models/enums/Eregion";

const AlbumDetail: React.FC<{ id: string | undefined }> = ({ id }) => {
    const [t] = useTranslation();
    const dispatch = useAppDispatch();

    useEffect(() => {
        dispatch(getAlbumDetailAsyncThunk({ id }));
        dispatch(getAllContentTypesAsyncThunk());
        dispatch(getAlbumAlertMessagesAsyncThunk());
    }, [dispatch, id]);

    const { albumAlertMessages, contentTypes } = useSelector((state: StoreState) => state.album);
    const { albumDetail } = useSelector((state: StoreState) => state.albumDetailCollection);
    const [region, setRegion] = useState<string>('vi');

    useEffect(() => {
        setRegion(albumDetail?.region === ERegion.en ? 'en' : 'vi');
    }, [albumDetail?.region])

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
        const listContentTypeIds = albumDetail?.contentTypeIds?.map(item => Number(item)) ?? [];
        return listContentTypeIds;
    }, [albumDetail]);

    // Use to build model
    const [albumAlertMessageSelectedOption, setAlbumAlertMessageSelectedOption] = useState<DropDownOption<number> | null>(
        albumAlertMessageDropDown.find(item => Number(item.value) === albumDetail?.albumAlertMessageId) ?? null
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
        reset
    } = useForm<AlbumDetailRequest>({
        values: useMemo((): AlbumDetailRequest => {
            setAlbumAlertMessageSelectedOption(albumAlertMessageDropDown.find(item => Number(item.value) === albumDetail?.albumAlertMessageId) ?? null);
            setContentTypesSelectedOptions(contentTypesDropDown.filter(item => contentTypeIds.includes(item.value)));

            return {
                title: albumDetail?.title ?? "",
                description: albumDetail?.description,
                createdOnUtc: albumDetail?.createdOnUtc ?? new Date(),
                updatedOnUtc: albumDetail?.updatedOnUtc,
                isPublic: albumDetail?.isPublic ?? false,
                region: region
            };
        }, [albumDetail, contentTypeIds, albumAlertMessageDropDown, contentTypesDropDown, region])
    });

    const onSubmit = async (albumRequestModel: AlbumDetailRequest) => {
        const toastId = toast.loading(t("toast.please_wait"), {
            hideProgressBar: true
        });

        if (albumDetail) {
            const response = await updateAlbumDetail(id, {
                ...albumRequestModel,
                albumAlertMessageId: albumAlertMessageSelectedOption ? Number(albumAlertMessageSelectedOption.value) : undefined,
                contentTypeIds: contentTypesSelectedOptions?.map(option => Number(option.value))
            });
            if (response.status === 200) {
                dispatch(getAlbumDetailAsyncThunk({ id }));

                toast.update(toastId, {
                    render: t("toast.update_sucessfully"),
                    isLoading: false,
                    type: toast.TYPE.SUCCESS,
                    autoClose: 2000
                });
                return;
            }
        }

        toast.done(toastId);
    };

    const onReset = () => {
        setRegion(albumDetail?.region === ERegion.en ? 'en' : 'vi');
        setAlbumAlertMessageSelectedOption(albumAlertMessageDropDown.find(item => Number(item.value) === albumDetail?.albumAlertMessageId) ?? null);
        setContentTypesSelectedOptions(contentTypesDropDown.filter(item => contentTypeIds.includes(item.value)));

        reset({
            title: albumDetail?.title,
            description: albumDetail?.description,
            albumAlertMessageId: albumDetail?.albumAlertMessageId,
            contentTypeIds: contentTypeIds,
            createdOnUtc: albumDetail?.createdOnUtc,
            updatedOnUtc: albumDetail?.updatedOnUtc,
            isPublic: albumDetail?.isPublic
        });
    }

    const onClearCache = async () => {
        const toastId = toast.loading(t("toast.please_wait"), {
            hideProgressBar: true
        });

        await clearCacheComicPage(albumDetail?.friendlyName ?? '');

        toast.update(toastId, {
            render: t("toast.update_sucessfully"),
            isLoading: false,
            type: toast.TYPE.SUCCESS,
            autoClose: 2000
        });
        return;
    }

    return (
        <div className="row">
            <div className="col-12">
                <div className="card">
                    <div className="card-header">
                        <div className="row align-items-center">
                            <div className="col">
                                <h4 className="card-title">{t('album_detail.title_detail')}</h4>
                            </div>
                            {/*end col*/}
                            <div className="col-auto">
                                <button className="btn"
                                    type="button"
                                    data-bs-toggle="collapse"
                                    data-bs-target="#collapseAlbumDetail"
                                    aria-expanded="true"
                                    aria-controls="collapseAlbumDetail">
                                    <i className="fa-solid fa-arrow-down text-secondary font-16"></i>
                                </button>
                            </div>
                            {/*end col*/}
                        </div>{" "}
                        {/*end row*/}
                    </div>
                    {/*end card-header*/}
                    <div className="card-body collapse show" id="collapseAlbumDetail">
                        <div className="row">
                            <div className="col-lg-6">
                                <div className="mb-3 row">
                                    <label
                                        className="col-sm-2 text-muted text-end"
                                    >
                                        {t('album_detail.is_public')}
                                    </label>
                                    <div className="col-sm-10">
                                        <input
                                            className="col-sm-2 form-check-input"
                                            type="checkbox"
                                            {...register("isPublic")}
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="row">
                            <div className="col-lg-6">
                                <div className="mb-3 row">
                                    <label
                                        className="col-sm-2 col-form-label text-end"
                                    >
                                        {t('album.modal.title')}
                                    </label>
                                    <div className="col-sm-10">
                                        <input
                                            className="form-control"
                                            type="text"
                                            {...register("title", { required: true })}
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
                                        className="col-sm-2 col-form-label text-end"
                                    >
                                        {t('album.modal.description')}
                                    </label>
                                    <div className="col-sm-10">
                                        <input
                                            className="form-control"
                                            type="email"
                                            {...register("description")}
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
                            </div>
                            <div className="col-lg-6">
                                <div className="mb-3 row">
                                    <label
                                        className="col-sm-2 col-form-label text-end"
                                    >
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
                                        className="col-sm-2 col-form-label text-end"
                                    >
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
                        <div>
                            <button
                                type="button"
                                className="btn btn-primary"
                                onClick={handleSubmit(onSubmit)}
                            >
                                {t('album_detail.button_save')}
                            </button>
                            <button
                                type="button"
                                className="btn btn-light"
                                onClick={onReset}
                            >
                                {t('album_detail.button_cancel')}
                            </button>
                            {albumDetail && <button
                                type="button"
                                className="btn btn-danger float-end"
                                onClick={onClearCache}
                            >
                                {t('album_detail.clear_cache')}
                            </button>}
                        </div>
                        {/*end card-body*/}
                    </div>
                    {/*end card*/}
                </div>{" "}
                {/*end col*/}
            </div>
            {/*end row*/}
        </div>
    );
}

export default React.memo(AlbumDetail);