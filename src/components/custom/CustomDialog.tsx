import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import type { ReactNode } from "react";

interface CustomDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: string;
  children: ReactNode;
  onConfirm: () => void;
  onCancel?: () => void;
  confirmLabel?: string;
  confirmDisabled?: boolean;
  isSubmitting?: boolean;
  submittingLabel?: string;
}

export function CustomDialog({
  open,
  onOpenChange,
  title,
  description,
  children,
  onConfirm,
  onCancel,
  confirmLabel = "Confirm",
  confirmDisabled = false,
  isSubmitting = false,
  submittingLabel,
}: CustomDialogProps) {
  const handleCancel = () => {
    if (onCancel) {
      onCancel();
    } else {
      onOpenChange(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="rounded-md">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && <DialogDescription>{description}</DialogDescription>}
        </DialogHeader>
        <div className="py-2">{children}</div>
        <DialogFooter>
          <Button variant="ghost" className="rounded-md" onClick={handleCancel}>
            Cancel
          </Button>
          <Button
            className="rounded-md"
            disabled={confirmDisabled || isSubmitting}
            onClick={onConfirm}
          >
            {isSubmitting && submittingLabel ? submittingLabel : confirmLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
