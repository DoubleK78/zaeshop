import React, { useState } from "react";
import ContentItemModel from "../../models/content-item/ContentItemModel";
import { LazyLoadImage } from 'react-lazy-load-image-component';
import { ActualFileObject, FilePondFile } from "filepond";
import ModalCommon from "../shared/ModalCommon";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import classNames from "classnames";
import { FilePond } from "react-filepond";
import { ContentItemBulkUploadItemModel } from "../../models/content-item/ContentItemBulkUploadModel";
import { RegexHelper } from "../../utils/regex";

type ContentItemBulkUploadItemProps = {
    contentItem: ContentItemModel;
    contentItemBulkUploadItemModel: ContentItemBulkUploadItemModel;
    updateExistItem: (id: number, isPublic: boolean, file?: ActualFileObject) => Promise<void>;
    deleteExistItem: (id: number) => void;
    isLazyLoading: boolean;
}

type ContentItemBlankUploadItemDialogProps = {
    modalIsOpen: boolean;
    openModal: () => void;
    closeModal: () => void;
    contentItem: ContentItemModel;
    updateExistItem: (id: number, isPublic: boolean, file?: ActualFileObject) => Promise<void>;
}

const ContentItemBlankUploadItemDialog: React.FC<ContentItemBlankUploadItemDialogProps> = ({ closeModal, contentItem, updateExistItem }) => {
    const [t] = useTranslation();
    const [files, setFiles] = useState<(ActualFileObject)[]>([]);

    const onUpdateFiles = (filesPondFiles: FilePondFile[]) => {
        const files = filesPondFiles.map(item => item.file);
        setFiles(files);
    }

    const onSubmit = async (model: ContentItemModel & { isPublic: boolean }) => {
        await updateExistItem(model.id, model.isPublic, files[0]);
        closeModal();
    }

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<ContentItemModel & { isPublic: boolean }>({
        defaultValues: {
            id: contentItem.id,
            name: contentItem.name,
            orderBy: RegexHelper.getNumberByText(contentItem.name),
            type: contentItem.type,
            createdOnUtc: contentItem.createdOnUtc,
            relativeUrl: contentItem.relativeUrl,
            originalUrl: contentItem.originalUrl,
            displayUrl: contentItem.displayUrl,
            isPublic: false
        }
    });

    return (
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
                                {t('content_item.update_image')}
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
                                        className="col-sm-3 text-muted text-end"
                                    >
                                        {t('content_item.is_public')}
                                    </label>
                                    <div className="col-sm-9">
                                        <input
                                            className="col-sm-2 form-check-input"
                                            type="checkbox"
                                            {...register("isPublic")}
                                        />
                                    </div>
                                </div>
                                <div className="mb-3 row">
                                    <label
                                        className="col-sm-3 col-form-label text-end">
                                        {t('content_item.order_by')}
                                    </label>
                                    <div className="col-sm-9">
                                        <input
                                            className="form-control"
                                            type="number"
                                            readOnly={true}
                                            {...register('orderBy', { min: 0, valueAsNumber: true })}
                                        />
                                        <div className={classNames("invalid-feedback", {
                                            "d-inline": errors.orderBy
                                        })}>
                                            <p>{t('content_item.order_by')}</p>
                                        </div>
                                    </div>
                                </div>

                                <div className="mb-3 row">
                                    <FilePond
                                        files={files}
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
    );
}

const ContentItemBlankUploadItem: React.FC<ContentItemBulkUploadItemProps> = ({ contentItem, contentItemBulkUploadItemModel, updateExistItem, deleteExistItem, isLazyLoading }) => {
    const [isOpen, setIsOpen] = useState(false);

    const openModal = () => setIsOpen(true);
    const closeModal = () => setIsOpen(false);

    return (
        <>
            <li className="list-group-item">
                {isLazyLoading && <LazyLoadImage
                    style={{
                        maxWidth: "100%",
                        height: "auto",
                    }}
                    src={contentItemBulkUploadItemModel.base64File ?? contentItem.displayUrl}
                    alt={contentItem.name}
                    threshold={75}
                    className="rounded d-block mx-auto"
                />}
                {!isLazyLoading && <img
                    style={{
                        maxWidth: "100%",
                        height: "auto",
                    }}
                    src={contentItemBulkUploadItemModel.base64File ?? contentItem.displayUrl}
                    alt={contentItem.name}
                    className="rounded d-block mx-auto"
                />}
                
                <button className="btn"
                    onClick={openModal}>
                    <i className="fa-solid fa-pen text-secondary font-16"></i>
                </button>
                <button className="btn"
                    onClick={() => deleteExistItem(contentItem.id)}>
                    <i className="fa-solid fa-trash text-danger font-16"></i>
                </button>
                <span className="ms-2 float-end text-muted">{contentItemBulkUploadItemModel.orderBy}</span>
            </li>
            <ModalCommon
                props={{ modalIsOpen: isOpen, openModal, closeModal, contentItem, updateExistItem }}
                Component={ContentItemBlankUploadItemDialog}
            />
        </>
    );
};

export default React.memo(ContentItemBlankUploadItem);