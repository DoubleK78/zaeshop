import { ActualFileObject, FilePondFile } from "filepond";
import React, { useState } from "react";
import { FilePond } from "react-filepond";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";
import { bulkUploadByLocalServer, createContentItems } from "../../services/content-item/cotentItemService";
import { RegexHelper } from "../../utils/regex";
import { ContentItemUploadLocalServer } from "../../models/content-item/ContentItemBulkUploadModel";

type ContentItemBlankUploadProps = {
    id: string | undefined;
    isCloudServer: boolean;
}

const ContentItemBlankUpload: React.FC<ContentItemBlankUploadProps> = ({ id, isCloudServer }: ContentItemBlankUploadProps) => {
    const [t] = useTranslation();

    const [files, setFiles] = useState<(ActualFileObject)[]>([]);

    const onUpdateFiles = (filesPondFiles: FilePondFile[]) => {
        const files = filesPondFiles.sort((a, b) => RegexHelper.getNumberByText(a.filename) - RegexHelper.getNumberByText(b.filename)).map(item => item.file);
        setFiles(files);
    }

    const onSubmit = async () => {
        if (files.length === 0) {
            toast.warning(t("toast.please_select_at_least_one_file_to_upload"), {
                hideProgressBar: true,
                autoClose: 2000
            })
            return;
        }

        if (isCloudServer) {
            const formData = new FormData();
            for (const element of files) {
                formData.append("files", element);
            }

            const toastId = toast.loading(t("toast.please_wait"), {
                hideProgressBar: true
            });

            if (id && Number(id)) {
                await createContentItems(Number(id), formData);

                toast.update(toastId, {
                    render: t("toast.create_inprogress_sucessfully"), type: toast.TYPE.SUCCESS, isLoading: false,
                    autoClose: 2000
                });

                return;
            }

            toast.done(toastId);
        }
        else {
            const model = files.map(file => {
                const item: ContentItemUploadLocalServer = {
                    fileName: file.name,
                    isPublic: false
                };
                return item;
            });

            const toastId = toast.loading(t("toast.please_wait"), {
                hideProgressBar: true
            });

            if (id && Number(id)) {
                await bulkUploadByLocalServer(Number(id), model);

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
    }

    return (
        <div className="row">
            <div className="row">
                <div className="col-12">
                    <div className="card">
                        <div className="card-header">
                            <div className="row align-items-center">
                                <div className="col">
                                    <h4 className="card-title">{t('content_item.title')}</h4>
                                    <p className="text-muted mb-0">{t('content_item.description')}</p>
                                </div>
                                {/*end col*/}
                            </div>{" "}
                            {/*end row*/}
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
                                onClick={onSubmit}
                            >
                                {t('content_item.upload_button')}
                            </button>
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

export default React.memo(ContentItemBlankUpload);