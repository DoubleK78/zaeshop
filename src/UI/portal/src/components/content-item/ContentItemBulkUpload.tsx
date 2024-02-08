import React, { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import ContentItemModel from "../../models/content-item/ContentItemModel";
import { v4 as uuidv4 } from 'uuid';
import ContentItemBulkUploadItem from "./ContentItemBulkUploadItem";
import { ActualFileObject, FilePondFile } from "filepond";
import { ContentItemBulkUploadItemModel, ContentItemUploadLocalServer } from "../../models/content-item/ContentItemBulkUploadModel";
import { FilePond } from "react-filepond";
import convertFileToBase64 from "../../utils/covert-base64";
import { toast } from "react-toastify";
import { bulkUpdateByLocalServer, bulkUpdateContentItems } from "../../services/content-item/cotentItemService";
import { useAppDispatch } from "../../store";
import { getContentItemsAsyncThunk } from "../../store/reducers/ContentItemSlice";
import { RegexHelper } from "../../utils/regex";

type ContentItemBulkUploadProps = {
    id: string | undefined;
    contentItems: ContentItemModel[];
    isCloudServer: boolean;
}

const ContentItemBulkUpload: React.FC<ContentItemBulkUploadProps> = ({ id, contentItems, isCloudServer }: ContentItemBulkUploadProps) => {
    const [t] = useTranslation();
    const dispatch = useAppDispatch();

    const [files, setFiles] = useState<(ActualFileObject)[]>([]);

    const onUpdateFiles = (filesPondFiles: FilePondFile[]) => {
        const files = filesPondFiles.sort((a, b) => RegexHelper.getNumberByText(a.filename) - RegexHelper.getNumberByText(b.filename)).map(item => item.file);
        setFiles(files);
    }

    const [contentItemBulkUploadItems, setContentItemBulkUploadItems] = useState<ContentItemBulkUploadItemModel[]>([]);

    useEffect(() => {
        if (!contentItems) return;
        const exsitsItems = [...contentItems].sort((a, b) => RegexHelper.getNumberByText(a.name) - RegexHelper.getNumberByText(b.name)).map(item => {
            return {
                id: item.id,
                fileName: item.name,
                isPublic: false,
                orderBy: RegexHelper.getNumberByText(item.name)
            };
        });

        setContentItemBulkUploadItems(exsitsItems);
    }, [contentItems]);

    const updateExistItem = async (id: number, isPublic: boolean, file?: ActualFileObject) => {
        const existsItem = contentItemBulkUploadItems.find(item => item.id === id);
        if (existsItem) {
            existsItem.isPublic = isPublic;
            existsItem.orderBy = RegexHelper.getNumberByText(existsItem.fileName);

            if (file) {
                const base64File = await convertFileToBase64(file);

                existsItem.fileName = file.name;
                existsItem.base64File = base64File;
            }

            setContentItemBulkUploadItems([...contentItemBulkUploadItems.filter(item => item.id !== id), existsItem].sort((a, b) => RegexHelper.getNumberByText(a.fileName) - RegexHelper.getNumberByText(b.fileName)));
        }
    }

    const deleteExistItem = (id: number) => {
        const confirm = window.confirm(t("content_item.confirm_delete_item"));
        if (confirm) {
            setContentItemBulkUploadItems(contentItemBulkUploadItems.filter(item => item.id !== id));
        }
    }

    const onSaveChanges = async () => {
        if (isCloudServer) {
            // Build exist items
            const existsItems: ContentItemBulkUploadItemModel[] = contentItemBulkUploadItems.map(item => {
                return {
                    id: item.id,
                    fileName: item.fileName,
                    base64File: item.base64File?.split(',')[1],
                    isPublic: item.isPublic,
                    orderBy: RegexHelper.getNumberByText(item.fileName)
                };
            });

            // Build new items
            const items: ContentItemBulkUploadItemModel[] = await Promise.all(files.map(async (item) => {
                const base64File = await convertFileToBase64(item);

                const newItem: ContentItemBulkUploadItemModel = {
                    fileName: item.name,
                    base64File: base64File?.split(',')[1],
                    isPublic: false,
                    orderBy: RegexHelper.getNumberByText(item.name)
                };
                return newItem;
            }));

            const toastId = toast.loading(t("toast.please_wait"), {
                hideProgressBar: true
            });

            if (id && Number(id)) {
                await bulkUpdateContentItems(Number(id), {
                    items,
                    existsItems
                });

                toast.update(toastId, {
                    render: t("toast.create_inprogress_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
                    autoClose: 2000
                });

                return;
            }

            toast.done(toastId);
        }
        else {
            const newItems = files.map(file => {
                const item: ContentItemUploadLocalServer = {
                    fileName: file.name,
                    isPublic: false
                };
                return item;
            });

            const existsItem = contentItemBulkUploadItems.map(file => {
                const item: ContentItemUploadLocalServer = {
                    id: file.id,
                    fileName: file.fileName,
                    isPublic: file.isPublic
                };
                return item;
            })

            const toastId = toast.loading(t("toast.please_wait"), {
                hideProgressBar: true
            });

            if (id && Number(id)) {
                await bulkUpdateByLocalServer(Number(id), [...newItems, ...existsItem]);

                toast.update(toastId, {
                    render: t("toast.create_inprogress_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
                    autoClose: 2000
                });

                return;
            }

            toast.done(toastId);
        }
    }

    const onReset = () => {
        setFiles([]);
        setContentItemBulkUploadItems([...contentItems].sort((a, b) => RegexHelper.getNumberByText(a.name) - RegexHelper.getNumberByText(b.name)).map(item => {
            return {
                id: item.id,
                fileName: item.name,
                isPublic: false,
                orderBy: RegexHelper.getNumberByText(item.name)
            };
        }));

        // Scroll on Top
        window.scrollTo(0, 0);
    }

    const onDeleteAll = async () => {
        const confirm = window.confirm(t("content_item.confirm_delete_all"));

        if (confirm) {
            setFiles([]);
            setContentItemBulkUploadItems([]);

            const toastId = toast.loading(t("toast.please_wait"), {
                hideProgressBar: true
            });

            await bulkUpdateContentItems(Number(id), {});

            toast.update(toastId, {
                render: t("toast.create_inprogress_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
                autoClose: 2000
            });

            // Referesh
            await dispatch(getContentItemsAsyncThunk({ id: Number(id) }));
        }
    }

    return (
        <div className="row">
            <div className="card">
                <div className="card-header">
                    <div className="form-group mb-3">
                        <h3 className="card-title">{t('content_item.bulk_upload_title')}</h3>
                        <p className="text-muted mb-0">
                            {t('content_item.bulk_upload_description')}
                        </p>
                    </div>
                    <div className="form-group mb-1">
                        <button
                            type="button"
                            className="btn btn-light"
                            onClick={onReset}
                        >
                            {t('content_item.reset')}
                        </button>
                        <button
                            type="button"
                            className="btn btn-primary"
                            onClick={onSaveChanges}
                        >
                            {t('content_item.bulk_upload_button')}
                        </button>

                        <button className="btn float-end"
                            onClick={onDeleteAll}>
                            <i className="fa-solid fa-trash text-danger font-16"></i>
                        </button>
                    </div>
                </div>
                {/*end card-header*/}
                <div className="card-body">
                    <div className="d-grid">
                        <FilePond
                            files={files}
                            onupdatefiles={onUpdateFiles}
                            allowMultiple={true}
                            maxFiles={250}
                            name="files"
                            labelIdle='Drag & Drop your files or <span class="filepond--label-action">Browse</span>'
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
                    <div className="row">
                        <ul className="list-group">
                            {contentItemBulkUploadItems.map((item) => (
                                <ContentItemBulkUploadItem
                                    key={uuidv4()}
                                    contentItemBulkUploadItemModel={item}
                                    contentItem={contentItems.find(contentItem => contentItem.id === item.id)!}
                                    updateExistItem={updateExistItem}
                                    deleteExistItem={deleteExistItem}
                                />
                            ))}
                        </ul>
                        {/*end col*/}
                    </div>
                    {/*end row*/}
                    {/* container */}
                </div>
                {/* end page content */}
            </div>
        </div>
    );
}

export default React.memo(ContentItemBulkUpload);